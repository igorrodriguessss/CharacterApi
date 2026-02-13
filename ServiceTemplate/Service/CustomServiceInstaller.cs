using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Collections.Specialized;

namespace $safeprojectname$.Service
{
    public class CustomServiceInstaller : ServiceInstaller
    {
        public CustomServiceInstaller(Type serviceType)
        {
            foreach (ServiceAttribute attr in serviceType
                .GetCustomAttributes(typeof(ServiceAttribute), true))
            {
                if (!String.IsNullOrEmpty(attr.ServiceName))
                    ServiceName = attr.ServiceName;
                if (attr.ServiceArguments != null)
                    ServiceArguments = attr.ServiceArguments;

                StartType = attr.StartMode;
                AutoRestartAttempts = attr.AutoRestartAttempts;
                AutoRestartDelayMilliseconds = attr.AutoRestartDelayMilliseconds;
                ResetFailureDelaySeconds = attr.ResetFailureDelaySeconds;
            }

            foreach (DisplayNameAttribute attr in serviceType
                .GetCustomAttributes(typeof(DisplayNameAttribute), true))
                DisplayName = attr.DisplayName;

            foreach (DescriptionAttribute attr in serviceType
                .GetCustomAttributes(typeof(DescriptionAttribute), true))
                Description = attr.Description;

            List<ServiceAccessAttribute> aces = new List<ServiceAccessAttribute>();
            foreach (ServiceAccessAttribute attr in serviceType
                .GetCustomAttributes(typeof(ServiceAccessAttribute), true))
                aces.Add(attr);

            if (aces.Count > 0)
                ServiceAccess = aces.ToArray();
        }

        [DefaultValue("")]
        [Description("Gets or sets the command-line arguments provided to the service executable.")]
        public string ServiceArguments { get; set; }

        [DefaultValue(false)]
        [Description("Gets or sets a value to delay the service startup.")]
        public bool DelayAutoStart { get; set; }

        [DefaultValue(300)]
        [Description("Gets or sets the timeout in seconds for a service shutdown.")]
        public int ShutdownTimeoutSeconds { get; set; }

        [Description("Gets or sets the access control entries to be defined for the service.")]
        public ServiceAccessAttribute[] ServiceAccess { get; set; }

        [DefaultValue(0)]//no restart
        [Description("Gets or sets the number of times to automatically reset the service, 0, 1, 2, or 3.")]
        public int AutoRestartAttempts { get; set; }
        
        [DefaultValue(1000)]//1 second
        [Description("Gets or sets the milliseconds to delay between failure and automatic restart.")]
        public int AutoRestartDelayMilliseconds { get; set; }

        [DefaultValue(86400)]//24 hours
        [Description("Gets or sets the time in seconds after which to reset the failure count to zero if there are no failures.")]
        public int ResetFailureDelaySeconds { get; set; }

        public override string HelpText
        {
            get
            {
                StringWriter message = new StringWriter();
                message.WriteLine("Service Install Options:");

                foreach (PropertyInfo pi in GetType().GetProperties())
                {
                    object def = null;
                    foreach (DefaultValueAttribute a in pi.GetCustomAttributes(typeof(DefaultValueAttribute), true))
                        def = a.Value;
                    string desc = null;
                    foreach (DescriptionAttribute a in pi.GetCustomAttributes(typeof(DescriptionAttribute), true))
                        desc = a.Description;
                    
                    if(def != null && desc != null)
                    {
                        def = pi.GetValue(this, null) ?? def;

                        message.WriteLine("/{0}=[{1}]", pi.Name, def);
                        string text = ' ' + desc;
                        int pos;
                        while (text.Length > 75 && (pos = text.LastIndexOf(' ', 75)) >= 0)
                        {
                            message.WriteLine(text.Substring(0, pos));
                            text = text.Substring(pos);
                        }
                        message.WriteLine(text);
                        message.WriteLine();
                    }
                }

                return message.ToString();
            }
        }

        public string GetServiceName(StringDictionary parameters) { return GetServiceName(parameters, null, ServiceName); }
        private string GetServiceName(IDictionary savedState) { return GetServiceName(Context.Parameters, savedState, ServiceName); }
        private static string GetServiceName(StringDictionary parameters, IDictionary savedState, string defaultValue)
        {
            string name = parameters["ServiceName"];
            if (String.IsNullOrEmpty(name) && savedState != null)
                name = (string)savedState["ServiceName"];
            if (String.IsNullOrEmpty(name))
                name = defaultValue;

            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Missing required parameter 'ServiceName'.");
            return name;
        }

        private void FillSettings(StringDictionary arguments)
        {
            foreach (PropertyInfo pi in GetType().GetProperties())
            {
                if(arguments.ContainsKey(pi.Name))
                {
                    object value = Convert.ChangeType(arguments[pi.Name], pi.PropertyType,
                                                      System.Globalization.CultureInfo.InvariantCulture);
                    pi.SetValue(this, value, null);
                }
            }
        }

        public override void Install(IDictionary stateSaver)
        {
            FillSettings(Context.Parameters);
            stateSaver["ServiceName"] = ServiceName = GetServiceName((IDictionary)null);

            //run the install to completion
            base.Install(stateSaver);

            //now we need to augment the installation options
            using (ServiceController svc = new ServiceController(ServiceName))
            {
                Win32Services.SetServiceExeArgs(svc, Context.Parameters["assemblypath"], ServiceArguments);
                Win32Services.SetDelayAutostart(svc, DelayAutoStart);
                Win32Services.SetShutdownTimeout(svc, TimeSpan.FromSeconds(ShutdownTimeoutSeconds));

                if (ServiceAccess != null)
                {
                    Win32Services.SetAccess(svc, ServiceAccess);
                }

                if (AutoRestartAttempts > 0)
                {
                    Win32Services.SetRestartOnFailure(svc, AutoRestartAttempts, AutoRestartDelayMilliseconds, ResetFailureDelaySeconds);
                }
            }
        }

        public override void Commit(IDictionary savedState)
        {
            ServiceName = GetServiceName(savedState);
            base.Commit(savedState);
        }

        public override void Uninstall(IDictionary savedState)
        {
            ServiceName = GetServiceName(savedState);
            base.Uninstall(savedState);
        }

        public override void Rollback(IDictionary savedState)
        {
            ServiceName = GetServiceName(savedState);
            base.Rollback(savedState);
        }
    }
}
