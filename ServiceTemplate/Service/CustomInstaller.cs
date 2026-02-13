using System;
using System.Collections;
using System.ComponentModel;
using System.ServiceProcess;

namespace $safeprojectname$.Service
{
    [RunInstaller(true)]
    public class CustomInstaller : System.Configuration.Install.Installer
{
        private readonly ServiceProcessInstaller _installProcess;
        private readonly CustomServiceInstaller _installService;

        public CustomInstaller()
        {
            _installProcess = new ServiceProcessInstaller();
            _installProcess.Account = ServiceAccount.NetworkService;

            _installService = new CustomServiceInstaller(typeof(ServiceImplementation));

            //Remove built-in EventLogInstaller:
            _installService.Installers.Clear(); 

            Installers.Add(_installProcess);
            Installers.Add(_installService);
        }

        public override void Install(IDictionary stateSaver)
        {
            if (Context.Parameters.ContainsKey("username"))
                _installProcess.Account = ServiceAccount.User;

            string svcName = _installService.GetServiceName(Context.Parameters);
            if (String.IsNullOrEmpty(svcName))
                throw new ArgumentException("Missing required parameter 'ServiceName'.");

            string svcArgs = Context.Parameters["ServiceArguments"] ?? _installService.ServiceArguments;
            _installService.ServiceArguments = ("-service " + svcName + " " + svcArgs).Trim();
            Context.Parameters.Remove("ServiceArguments");

            //run the install
            base.Install(stateSaver);
        }
    }
}

