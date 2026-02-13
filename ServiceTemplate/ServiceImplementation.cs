using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using $safeprojectname$.Service;
using System.Security.Principal;
using System.Security.AccessControl;
using $safeprojectname$.Resources;

namespace $safeprojectname$
{
    [DisplayName("$safeprojectname$")]
    //[Description("$safeprojectname$")]
    [Service("$safeprojectname$", StartMode = ServiceStartMode.Automatic, AutoRestartAttempts = 2)]
    [ServiceAccess(WellKnownSidType.BuiltinAdministratorsSid, AceQualifier.AccessAllowed, ServiceAccessRights.SERVICE_ALL_ACCESS)]
    class ServiceImplementation : MarshalByRefObject, IDisposable /* optional support: , IPauseAndContinue */
    {
        private readonly string _serviceName;
        public ServiceImplementation(string serviceName)
        {
            _serviceName = serviceName;
        }

        public void Dispose()
        {
        }

        public void Start(string[] arguments)
        {
            Trace.WriteLine(FormatString.StartingService);
        }

        public void Stop()
        {
            Trace.WriteLine(FormatString.StoppingService);
        }

        public void ExecuteCommand(int command)
        {
            throw new NotImplementedException();
        }

        #region Advanced Service API
        public void Pause()
        { /* to implement, uncomment the IPauseAndContinue interface */ }
        public void Continue()
        { /* to implement, uncomment the IPauseAndContinue interface */ }

        public void Shutdown()
        {
            Stop();
        }

        public void SessionChange(SessionChangeReason changeDescription, int sessionId)
        {
        }

        public bool CanSuspend()
        {
            return true;
        }

        public void PowerEvent(PowerBroadcastStatus powerStatus)
        {
        }
        #endregion

        #region Overrides
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public override string ToString()
        {
            return String.Format("{0}({1})", GetType(), _serviceName);
        }
        #endregion
    }
}
