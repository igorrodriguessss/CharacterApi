namespace $safeprojectname$.Resources
{
    /// <summary>
    /// Installer class for the Event Log messages
    /// </summary>
    [global::System.ComponentModel.RunInstaller(true)]
    [global::System.Diagnostics.DebuggerStepThroughAttribute()]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("CSharpTest.Net.Generators", "1.11.924.348")]
    public class Installer : System.Configuration.Install.Installer
    {
        readonly System.Diagnostics.EventLogInstaller _installServiceTemplateServiceTemplate;
        
        /// <summary>
        /// Constructs the installer for the Event Log
        /// </summary>
        public Installer()
        {
            _installServiceTemplateServiceTemplate = new System.Diagnostics.EventLogInstaller();
            _installServiceTemplateServiceTemplate.Log = @"$safeprojectname$";
            _installServiceTemplateServiceTemplate.Source = @"$safeprojectname$";
            _installServiceTemplateServiceTemplate.CategoryCount = 2;
            _installServiceTemplateServiceTemplate.UninstallAction = System.Configuration.Install.UninstallAction.Remove;
            Installers.Add(_installServiceTemplateServiceTemplate);
            
        }
        
        /// <summary>
        /// Customizes the MessageResourceFile durring installation
        /// </summary>
        public override void Install(System.Collections.IDictionary state)
        {
            _installServiceTemplateServiceTemplate.CategoryResourceFile = _installServiceTemplateServiceTemplate.MessageResourceFile =
                System.IO.Path.GetFullPath(Context.Parameters["assemblypath"].Trim('"'));
            
            base.Install(state);
            
            using (System.Diagnostics.EventLog log = new System.Diagnostics.EventLog(@"$safeprojectname$", "."))
            {
                log.MaximumKilobytes = 1024 * 10;
                log.ModifyOverflowPolicy(System.Diagnostics.OverflowAction.OverwriteAsNeeded, 30);
            }
        }
    }
}
