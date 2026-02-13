using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;

namespace $safeprojectname$.Service
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ServiceAttribute : Attribute
    {
        public readonly string ServiceName;
        public string ServiceArguments;
        public ServiceStartMode StartMode = ServiceStartMode.Manual;
        public int AutoRestartAttempts = 0;
        public int AutoRestartDelayMilliseconds = 1000;
        public int ResetFailureDelaySeconds = 86400;

        public ServiceAttribute() : this(null) { }
        public ServiceAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }

        public static string GetServiceName(Type type)
        {
            foreach (ServiceAttribute a in type.GetCustomAttributes(typeof(ServiceAttribute), true))
                return a.ServiceName;
            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ServiceAccessAttribute : Attribute
    {
        public readonly WellKnownSidType Sid;
        public readonly AceQualifier Qualifier;
        public readonly ServiceAccessRights AccessMask;

        public ServiceAccessAttribute(WellKnownSidType sid, AceQualifier qualifier, ServiceAccessRights accessMask)
        {
            Sid = sid;
            Qualifier = qualifier;
            AccessMask = accessMask;
        }
    }
}
