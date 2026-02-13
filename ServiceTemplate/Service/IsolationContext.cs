using System;
using System.Globalization;
using $safeprojectname$.Properties;

namespace $safeprojectname$.Service
{
    class IsolationContext : IDisposable
    {
        private AppDomain _childDomain;

        public IsolationContext(string serviceName)
        {
            if(Settings.Default.IsolateDomain)
            {
                AppDomainSetup config = new AppDomainSetup();
                config.ApplicationName = serviceName;
                config.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                config.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

                if(Settings.Default.ShadowCopy)
                {
                    config.ShadowCopyDirectories = AppDomain.CurrentDomain.BaseDirectory;
                    config.ShadowCopyFiles = "true";
                }

                _childDomain = AppDomain.CreateDomain(serviceName, AppDomain.CurrentDomain.Evidence, config);
            }
        }

        public void Dispose()
        {
            if (_childDomain != null)
                try { AppDomain.Unload(_childDomain); } catch { }

            _childDomain = null;
        }

        public T CreateInstance<T>(params object[] arguments)
        {
            AppDomain domain = _childDomain ?? AppDomain.CurrentDomain;
            object oinstance = domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false,
                                                              System.Reflection.BindingFlags.CreateInstance, null,
                                                              arguments, CultureInfo.CurrentCulture, null,
                                                              domain.Evidence);
            return (T) oinstance;
        }
    }
}
