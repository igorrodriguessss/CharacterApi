using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using $safeprojectname$.Service;
using System.Threading;
using System.Security.AccessControl;
using $safeprojectname$.Resources;
using $safeprojectname$.Utilities;

namespace $safeprojectname$
{
    internal static class Commands
    {
        public static void Help(ICollection<string> args)
        {
            #region string helpText = "...";
            string helpText = @"
Usage:
  {0} [/nologo] Command-Name [Arguments]

Commands:
  Help      Displays this help content.
  Install   Installs the service, use Install /? for a list of options.
  Uninstall Removes the service, event log, and all event log entries.

The following commands allow the service name to be provided as the first 
parameter on the command-line.

  Run       Runs the service as a console application.
  Start     Starts the service, optionally with service name and arguments.
  Stop      Stops the service, optionally with service name.
  Restart   Stops and Starts the service, optionally with service name.
  Command   Executes a numeric command on the service.
"
#if DEBUG
+ @"
Debugging note: To debug the service you may run the Start command while
specifying the service name and arguments followed by 'DEBUG!'.
"
#endif
;
            #endregion
            Console.WriteLine(helpText, System.IO.Path.GetFileName(typeof(Program).Assembly.Location));
        }

        public static void Run(ICollection<string> argsIn)
        {
            List<string> args = new List<string>(argsIn);

            string svcName = ServiceAttribute.GetServiceName(typeof(ServiceImplementation));
            if (args.Count > 0)
                svcName = args[0];

            if (String.IsNullOrEmpty(svcName))
                throw new ArgumentException(FormatString.MissingRequiredArgument, FormatString.ServiceName);

            Trace.Listeners.Add(new ConsoleTraceListener(false));
            using (ServiceImplementation svc = new ServiceImplementation(svcName))
            {
                svc.Start(new List<string>(args).ToArray());

                Console.WriteLine(FormatString.PressEnterToExit);
                Console.ReadLine();

                svc.Stop();
            }
        }

        public static void RunAsService(ICollection<string> args)
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ServiceBase.Run(new Service.ServiceProcess(args));
        }

        #region Elevated Installation
        public static string EscapeArguments(params string[] args)
        {
            StringBuilder arguments = new StringBuilder();
            Regex invalidChar = new Regex("[\x00\x0a\x0d]");//  these can not be escaped
            Regex needsQuotes = new Regex(@"\s|""");//          contains whitespace or two quote characters
            Regex escapeQuote = new Regex(@"(\\*)(""|$)");//    one or more '\' followed with a quote or end of string
            for (int carg = 0; args != null && carg < args.Length; carg++)
            {
                if (args[carg] == null) { throw new ArgumentNullException("args[" + carg + "]"); }
                if (invalidChar.IsMatch(args[carg])) { throw new ArgumentOutOfRangeException("args[" + carg + "]"); }
                if (args[carg] == String.Empty) { arguments.Append("\"\""); }
                else if (!needsQuotes.IsMatch(args[carg])) { arguments.Append(args[carg]); }
                else
                {
                    arguments.Append('"');
                    arguments.Append(escapeQuote.Replace(args[carg], m =>
                    m.Groups[1].Value + m.Groups[1].Value +
                    (m.Groups[2].Value == "\"" ? "\\\"" : "")
                    ));
                    arguments.Append('"');
                }
                if (carg + 1 < args.Length)
                    arguments.Append(' ');
            }
            return arguments.ToString();
        }

        static bool ContinueAsAdmin()
        {
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                return true;//already admin

            List<string> args = new List<string>(Environment.GetCommandLineArgs());
            string exe = args[0];
            args.RemoveAt(0);
            args.Add("/wait");

            ProcessStartInfo psi = new ProcessStartInfo(exe,  EscapeArguments(args.ToArray()));
            psi.WorkingDirectory = Environment.CurrentDirectory;
            psi.UseShellExecute = true;
            psi.Verb = "RunAs";

            try
            {
                Process result = Process.Start(psi);
                result.WaitForExit();

                if (result.ExitCode != 0)
                    Console.Error.WriteLine("The install process returned error code {0}.", result.ExitCode);
            }
            catch(OperationCanceledException)
            { }
            return false;
        }
        #endregion

        public static void Install(ICollection<string> args)
        {
            if (ContinueAsAdmin())
            {
                new InstallUtil(typeof (Program).Assembly)
                    .Install(args);
            }
        }

        public static void Uninstall(ICollection<string> args)
        {
            if (ContinueAsAdmin())
            {
                new InstallUtil(typeof (Program).Assembly)
                    .Uninstall(args);
            }
        }

        public static void Start(ICollection<string> argsIn)
        {
            List<string> args = new List<string>(argsIn);

            string svcName = ServiceAttribute.GetServiceName(typeof(ServiceImplementation));
            if (args.Count > 0)
                args.Remove(svcName = args[0]);

            if (String.IsNullOrEmpty(svcName))
                throw new ArgumentException(FormatString.MissingRequiredArgument, FormatString.ServiceName);

            using (ServiceController ctrl = new ServiceController(svcName))
            {
                if (ctrl.Status == ServiceControllerStatus.Running)
                    return;

                Console.WriteLine(FormatString.StartingService);
                ctrl.Start(args.ToArray());
                do
                {
                    Thread.Sleep(1000);
                    ctrl.Refresh();
                } 
                while (ctrl.Status == ServiceControllerStatus.StartPending);

                if (ctrl.Status != ServiceControllerStatus.Running)
                    throw new ApplicationException(FormatString.UnableToControlService);
            }
        }

        public static void Stop(ICollection<string> argsIn)
        {
            List<string> args = new List<string>(argsIn);

            string svcName = ServiceAttribute.GetServiceName(typeof(ServiceImplementation));
            if (args.Count > 0)
                args.Remove(svcName = args[0]);

            if (String.IsNullOrEmpty(svcName))
                throw new ArgumentException(FormatString.MissingRequiredArgument, FormatString.ServiceName);

            using (ServiceController ctrl = new ServiceController(svcName))
            {
                if (ctrl.Status == ServiceControllerStatus.Stopped)
                    return;

                Console.WriteLine(FormatString.StoppingService);
                ctrl.Stop();
                do
                {
                    Thread.Sleep(1000);
                    ctrl.Refresh();
                }
                while (ctrl.Status == ServiceControllerStatus.StopPending);

                if (ctrl.Status != ServiceControllerStatus.Stopped)
                    throw new ApplicationException(FormatString.UnableToControlService);
            }
        }

        public static void Restart(ICollection<string> args)
        {
            Stop(args);
            Start(args);
        }

        public static void Command(ICollection<string> argsIn)
        {
            List<string> args = new List<string>(argsIn);

            int value = 0;
            string svcName = ServiceAttribute.GetServiceName(typeof (ServiceImplementation));
            if(args.Count == 2)
                args.Remove(svcName = args[0]);
        
            if (String.IsNullOrEmpty(svcName))
                throw new ArgumentException(FormatString.MissingRequiredArgument, FormatString.ServiceName);
            if (args.Count < 1)
                throw new ArgumentException(FormatString.MissingRequiredArgument, FormatString.CommandID);
            if (!int.TryParse(args[0], out value))
                throw new ArgumentException(FormatString.InvalidArgumentValue(args[0]), FormatString.CommandID);
            if (value < 128 || value > 256)
                throw new ArgumentOutOfRangeException(FormatString.CommandID, FormatString.ArgumentOutOfRange(128, 256));

            bool created;
            EventWaitHandleSecurity secpol = new EventWaitHandleSecurity();
            secpol.AddAccessRule(
                new EventWaitHandleAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    EventWaitHandleRights.FullControl, 
                    AccessControlType.Allow
                    ));

            using (Mutex commandLock = new Mutex(false, FormatString.ServiceCommandLock(svcName, value)))
            using (EventWaitHandle successSignal = new EventWaitHandle(false, EventResetMode.ManualReset, 
                   FormatString.CommandSuccessSignal(svcName, value), out created, secpol))
            using (EventWaitHandle failureSignal = new EventWaitHandle(false, EventResetMode.ManualReset, 
                   FormatString.CommandFailureSignal(svcName, value), out created, secpol))
            using (ServiceController ctrl = new ServiceController(svcName))
            {
                bool locked = false;
                try
                {
                    try { locked = commandLock.WaitOne(); }
                    catch (AbandonedMutexException) { locked = true; }

                    successSignal.Reset();
                    failureSignal.Reset();

                    ctrl.ExecuteCommand(value);

                    int response = WaitHandle.WaitAny(new[] {successSignal, failureSignal}, TimeSpan.FromMinutes(1), true);

                    if (response == WaitHandle.WaitTimeout)
                        throw new System.TimeoutException(FormatString.CommandTimeoutMessage);
                    if (response == 1)
                        throw new ApplicationException(FormatString.CommandFailedMessage);
                }
                finally
                {
                    if (locked)
                        commandLock.ReleaseMutex();
                }
            }
        }
    }
}