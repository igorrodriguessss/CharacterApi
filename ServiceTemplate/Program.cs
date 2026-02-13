using System;
using System.Collections.Generic;
using System.Reflection;

namespace $safeprojectname$
{
    class Program
    {
        private static readonly Dictionary<string, Action<ICollection<string>>> Actions
            = new Dictionary<string, Action<ICollection<string>>>(StringComparer.OrdinalIgnoreCase)
                  {
                      {"-service", Commands.RunAsService},
                      {"Help", Commands.Help},
                      {"Run", Commands.Run},
                      {"Install", Commands.Install},
                      {"Uninstall", Commands.Uninstall},
                      {"Start", Commands.Start},
                      {"Stop", Commands.Stop},
                      {"Restart", Commands.Restart},
                      {"Command", Commands.Command},
                  };

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception ?? new Exception();
            Resources.Messages.UnhandledError(ex.Message, ex);
        }

        static int Main(string[] rawArgs)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            int exitCode = 1;
            List<string> args = new List<string>(rawArgs);

            bool nologo = args.Remove("/nologo") || args.Remove("-nologo");
            bool wait = (args.Remove("/wait") || args.Remove("-wait")) && Environment.UserInteractive;

            try
            {
                string cmdName = String.Empty;
                if(args.Count > 0)
                {
                    cmdName = args[0];
                    args.RemoveAt(0);
                }

                if(!nologo)
                {
                    Console.WriteLine(typeof (Program).Assembly.FullName);
                    foreach(AssemblyCopyrightAttribute copyright in 
                        typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false))
                    {
                        Console.WriteLine(copyright.Copyright);
                    }
                }

                if (args.Count == 0 && !Environment.UserInteractive)
                    cmdName = "-service";

                Action<ICollection<string>> cmdAction;
                if (!Actions.TryGetValue(cmdName, out cmdAction))
                    cmdAction = Commands.Help;

                cmdAction(args);

                exitCode = 0;
            }
            catch (ApplicationException ex)
            {
                Console.Error.WriteLine("{0}: {1}", ex.GetType(), ex.Message);
            }
            catch (System.Threading.ThreadAbortException)
            { }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }

            if(wait)
            {
                Console.WriteLine(Resources.FormatString.PressEnterToExit);
                Console.ReadLine();
            }

            Environment.ExitCode = exitCode;
            return exitCode;
        }
    }
}
