using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Autodesk.Revit.DB;


namespace LuxExporter
{
    class Revit_Linked_Files : IEquatable<Revit_Linked_Files>
    {
        private String vLinkName = "";
        public String LinkName 
        { 
            get {return vLinkName;}
            set{vLinkName=value;}
        }
        
        private String vLinkShortName = "";
        public String LinkShortName
        {
            get { return vLinkShortName; }
            set { vLinkShortName = value; }
        }

        private Element vRevitLink;
        public Element RevitLink 
        {
            get { return vRevitLink; }
            set { vRevitLink = value; }
        }

        private String vGeoFilePath;
        public String GeoFilePath
        { 
            get { return vGeoFilePath; }
            set { vGeoFilePath = value; }
        }

        private String vUniqueLinkName;
        public String UniqueLinkName
        {
            get { return vUniqueLinkName; }
            set { vUniqueLinkName = value; }
        }

        private String vPLYFolderPath;
        public String PLYFolderPath
        {
            get { return vPLYFolderPath; }
            set { vPLYFolderPath = value; }
        }
        private List<Transform> vTransformations;
        public List<Transform> Transformations 
        {
            get { return vTransformations; }
        }


        //implement custom equals function

        public bool Equals(Revit_Linked_Files other)
        {
            if (this.LinkName == other.LinkName)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddTransForm(Transform t)
        {
            try
            {
                vTransformations.Add(t);
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in linked file class");
                throw;
            }
            
        }

        public Revit_Linked_Files(String LinkName)
        {
            //initiate transform list
            vTransformations = new List<Transform>();
            vLinkName = LinkName;
        }
    }
}
