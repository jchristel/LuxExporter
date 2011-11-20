using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.Utility;
namespace LuxExporter
{

    class Revit_Material_Worker
    {
        #region Properties
        //current Revit Document
        private Document vDoc;
        public Document CurrentRevitDocument
        {
            get { return vDoc; }
            set { vDoc = value; }
        }
        #endregion

        #region Exporter Functions

        public StringBuilder ExportMaterial(Autodesk.Revit.DB.Material RevitMaterial)
        {
            try
            {
                StringBuilder MaterialExport = new StringBuilder("");  
                //get material color
                Color MaterialColor = RevitMaterial.Color;
                //separate out RGB values and convert to luxrender values
                Double Red = MaterialColor.Red / 255.0;
                Double Green = MaterialColor.Green / 255.0;
                Double Blue = MaterialColor.Blue / 255.0;

                //ignore glow for time beeing
                //RevitMaterial.Glow
                
                //check transparency if any apply glass2 material
                if (RevitMaterial.Transparency > 0)
                {
                    MaterialExport.AppendLine("MakeNamedMaterial\"" + RevitMaterial.Name.ToString() + "\"");
                    MaterialExport.AppendLine("\"bool architectural\" [\"true\"]");
                    MaterialExport.AppendLine("\"bool dispersion\" [\"false\"]");
                    MaterialExport.AppendLine("\"string type\" [\"glass2\"]");

                    //Glass VOLUMNE
                    MaterialExport.AppendLine("MakeNamedVolume \""+ RevitMaterial.Name.ToString() +"\" \"clear\"");
                    MaterialExport.AppendLine("\"float fresnel\" [1.519999980926514]");
                    MaterialExport.AppendLine("\"color absorption\" [" + Red.ToString() + " " + Green.ToString() + " " + Blue.ToString() + "]");
                
                
                }
                else
                {
                    ////check shininess if any apply glossy material
                    //if (RevitMaterial.Shininess>0)
                    //{
                    //    MaterialExport.AppendLine("MakeNamedMaterial\"" + RevitMaterial.Name.ToString() + "\"");
                    //    MaterialExport.AppendLine("\"bool multibounce\" [\"false\"]");
                    //    MaterialExport.AppendLine("\"string type\" [\"glossy\"]");
                    //    MaterialExport.AppendLine("\"color Kd\" [" + Red.ToString() + " " + Green.ToString() + " " + Blue.ToString() + "]");
                    //    //Ks is hardcoded
                    //    MaterialExport.AppendLine("\"color Ks\" [0.04954654 0.04954654 0.04954654]");
                    //    MaterialExport.AppendLine("\"float index\" [0.000000000000000]");
                    //    //roughness is hardcoded could  RevitMaterial.Smoothness be used?
                    //    MaterialExport.AppendLine("\"float uroughness\" [0.050000000745058]");
                    //    MaterialExport.AppendLine("\"float vroughness\" [0.050000000745058]");
                    //}
                    //else
                    //{
                        

                        //assume all materials to be matte for time being
                        MaterialExport.AppendLine("MakeNamedMaterial\"" + RevitMaterial.Name.ToString() + "\"");
                        MaterialExport.AppendLine("\"string type\" [\"matte\"]");
                        MaterialExport.AppendLine("\"color Kd\" [" + Red.ToString() + " " + Green.ToString() + " " + Blue.ToString() + "]");
                        MaterialExport.AppendLine("");
                
                    //}
                }
               
                
                //Renderapperence is handled via asset class investigate further...
                //Asset RenderAppearence = RevitMaterial.RenderAppearance;
                
                
                
                return MaterialExport;
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in Material Export");
                return null;
                throw;
            }
        }
        #endregion

        #region class constructors
        //class constructor (takes Revit Element as argument)
        public Revit_Material_Worker(Document cDoc)
        {
            try
            {
                vDoc = cDoc;
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in module Revit_Material_Worker (Class Constructor)");
                throw;
            }
            
        }
        #endregion


    }
}
