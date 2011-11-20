using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace LuxExporter
{

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
   // [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class LuxExporter_Entry : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
        ref string message, ElementSet elements)
        {

            try
            {
                Autodesk.Revit.UI.UIApplication revitApp = revit.Application;
                //get the active document
                ExternalCommandData cdata = revit;
                Document doc = cdata.Application.ActiveUIDocument.Document;

                //UIDocument uidoc = cdata.Application.ActiveUIDocument;
                //Selection selection = uidoc.Selection;
                //ElementSet collection = selection.Elements;
                //if (0 == collection.Size)
                //{
                //    // If no elements selected.
                //    TaskDialog.Show("Revit", "You haven't selected any elements.");
                //}
                //else
                //{
                //    LuxExporter.Revit_Transform test = new Revit_Transform(collection);
                //}

                //get linked files 
                DocumentSet LinkedFiles = revitApp.Application.Documents;


                //create a Luxexporter
                LuxExporter_Main Exporter = new LuxExporter_Main();
                //set hard coded exporter output path
                Exporter.OutputFilePath = @"C:/temp/lux/";
                //get list of 3d views in file
                Revit_Filter ViewFilter = new Revit_Filter(doc);
                Exporter.ViewToExport = ViewFilter.GetThreeDViews();
                Exporter.CurrentRevitDocument = doc;
                Exporter.LinkedFiles = LinkedFiles;
                Exporter.ExportToLux();

                return Autodesk.Revit.UI.Result.Succeeded;
            }
            catch (Exception)
            {
                return Autodesk.Revit.UI.Result.Failed;
                
            }
            
        }
    }

}
