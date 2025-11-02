using Autodesk.Revit.UI;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TemplateProject
{
    public class App : IExternalApplication
    {
        #region Properties, fields,...
        private readonly string pathFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        private Autodesk.Revit.UI.RibbonPanel? _ribbonPanel = null!;
        private UIControlledApplication _app = null!;
        private string _tabName = "Your name";
        #endregion

        #region Interface events: IExternalApplication
        public Result OnStartup(UIControlledApplication application)
        {
            // IMPORTATNT: USE THIS TO LOAD AGAIN ASSEMBLY IF IT WAS NOT FOUND (NEED PAPER LIBRARY)
            //string dllFolder = Path.Combine(pathFolder);
            //AssemblyUtility.LoadAllRibbonAssemblies(dllFolder);

            // Assign value
            _app = application;

            // Create tabPanel
            InitializeTabPanel(_tabName);           

            // Create RibbonPanel          
            var commandRibbonPanel = application.CreateRibbonPanel(_tabName, "Command");

            // Register buttons onto ribbonPanel
            CommandRibbonPanel(commandRibbonPanel);

            return Result.Succeeded;
        }



        public Result OnShutdown(UIControlledApplication application)
        {
            //_ribbonPanel?.Remove();
            return Result.Succeeded;
        }
        #endregion

        #region Panels
        private void CommandRibbonPanel(RibbonPanel commandRibbonPanel)
        {
            return;
        }
        #endregion

        #region Other methods, events
        /// <summary>
        /// Handles the AppDomain.AssemblyResolve event to load embedded assembly resources.
        /// This enables the application to load dependencies packed inside itself (as resources),
        /// which is useful for single-file deployment or plugin loading.
        /// </summary>
        //        private Assembly? CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        //        {
        //            // Get assembly name
        //            string assemblyName = new AssemblyName(args.Name).Name + ".dll";

        //            // Get resource name
        //#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        //            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
        //                .Where(x => x.EndsWith(".dll"))
        //                .ToArray()
        //                .FirstOrDefault(x => x.EndsWith(assemblyName));
        //#pragma warning restore CS8600
        //            if (resourceName is null)
        //            {
        //                return null;
        //            }

        //            // Load assembly from resource
        //            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        //            {
        //                var bytes = new byte[stream!.Length];
        //                stream.Read(bytes, 0, bytes.Length);
        //                return Assembly.Load(bytes);
        //            }
        //        }
        #endregion

        #region Initialization
        private void InitializeTabPanel(string tabName)
        {
            try
            {
                _app.CreateRibbonTab(_tabName);

            }
            catch (Exception ex)
            {
                Debug.Print($"Error: {ex.Message}");
            }
        }
        #endregion
    }
}
