using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;

namespace TemplateProject.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            Autodesk.Revit.DB.View currentView = doc.ActiveView;

            MessageBox.Show("Hello world!");

            return Result.Succeeded;
        }
    }
}
