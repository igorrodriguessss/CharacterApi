using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace $safeprojectname$.Utilities
{
    class InstallUtil
    {
        private readonly AssemblyName _assemblyName;
        private readonly string _baseDirectory;
        private readonly string _fileName;
        private readonly string _installUtilExe;

        public InstallUtil(Assembly assembly)
        {
            _assemblyName = assembly.GetName();
            _fileName = Path.GetFullPath(assembly.Location);
            _baseDirectory = Path.GetDirectoryName(_fileName);

            _installUtilExe = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "InstallUtil.exe"));
            if (!File.Exists(_installUtilExe))
                throw new FileNotFoundException("InstallUtil.exe not found.", _installUtilExe);
        }

        public void Install(IEnumerable<string> moreargs)
        {
            if (0 != Run(true, moreargs))
                throw new ApplicationException(String.Format("InstallUtil failed to install {0}.", _assemblyName.Name));
        }

        public void Uninstall(IEnumerable<string> moreargs)
        {
            if (0 != Run(false, moreargs))
                Console.Error.WriteLine("InstallUtil failed to uninstall {0}.", _assemblyName.Name);
        }

        private int Run(bool install, IEnumerable<string> moreargs)
        {
            string apppath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string appdata = Path.Combine(apppath, _assemblyName.Name);
            if (!Directory.Exists(appdata))
                Directory.CreateDirectory(appdata);

            List<string> arguments = new List<string>(moreargs);
            if (!install)
                arguments.Add("/uninstall");
            arguments.AddRange(new [] 
                                   {
                                       "/ShowCallStack=true",
                                       String.Format("/LogToConsole=true"),
                                       String.Format("/LogFile={0}", Path.Combine(appdata, "install.log")),
                                       String.Format("/InstallStateDir={0}", _baseDirectory),
                                       String.Format("{0}", _fileName),
                                   });

            int result = AppDomain.CurrentDomain.ExecuteAssembly(
                    _installUtilExe,
                    AppDomain.CurrentDomain.Evidence,
                    arguments.ToArray()
                    );

            return result;
        }
    }
}
