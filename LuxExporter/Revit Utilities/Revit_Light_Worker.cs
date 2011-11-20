using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace LuxExporter
{
    class Revit_Light_Worker
    {
        #region Properties

        

        private Autodesk.Revit.DB.FamilyInstance vRevitElement;
        public Autodesk.Revit.DB.FamilyInstance RevitElement
        {
            get { return vRevitElement; }
            set { vRevitElement = value; }
        }

        //current Revit Document
        private Document vDoc;
        public Document CurrentRevitDocument
        {
            get { return vDoc; }
            set { vDoc = value; }
        }



        #endregion 

        #region Exporter Functions
        public StringBuilder ExportLight(Options GeometryOption)
        {
            try
            {
                //setup export string
                StringBuilder strExport = new StringBuilder("");
                //get the parent object this family was instanciated of
                ElementId typeID = vRevitElement.GetTypeId();
                Element Light = vDoc.get_Element(typeID);
                //get the parameterset
                ParameterSet LightParameters = Light.Parameters;
                foreach (Parameter item in LightParameters)
                {
                    strExport.AppendLine("ParameterName: " + item.Definition.Name.ToString() + " Value: " + item.AsString());
                }

                //System.Windows.Forms.MessageBox.Show(strExport.ToString());
                //return light data
                return strExport;
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in module Revit_Light_Worker (ExportLight) ");
                return null;
                throw;
            }
        }

        #endregion

        #region class constructors
        //class constructor (takes Revit Element as argument)
        public Revit_Light_Worker(Autodesk.Revit.DB.FamilyInstance objInstance, Document cDoc)
        {
            try
            {
                vRevitElement = objInstance;
                vDoc = cDoc;
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in module Revit_Light_Worker (Class Constructor)");
                throw;
            }
            
        }
        #endregion
    }
}
