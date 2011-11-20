using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.Revit.DB;

namespace LuxExporter
{
    class Revit_Geometry_Worker
    {
        #region Properties

        private StringBuilder strObjectName;
        
        private Autodesk.Revit.DB.Element vRevitElement;
        public Autodesk.Revit.DB.Element RevitElement 
        {
            get { return vRevitElement; }
            set { vRevitElement = value; }
        }


        private String vOutputFilePath;
        public String SceneFilePath 
        {
            get { return vOutputFilePath; }
            set { vOutputFilePath = value; }
        }

        //current Revit Document
        private Document vDoc;
        public Document CurrentRevitDocument
        {
            get { return vDoc; }
            set { vDoc = value; }
        }

        private Int32 vObjectCounter = 0;


        #endregion 

        #region Exporter Functions

        public StringBuilder ExportToPLY_Mesh(Options GeometryOption, Transform t)
        {
            try
            {
                GeometryElement element = vRevitElement.get_Geometry(GeometryOption);
                GeometryObjectArray geoObjectArray = element.Objects;

                //setup export string
                StringBuilder strMesh = new StringBuilder("");

                //setup mesh
                Mesh mesh = null;

                foreach (GeometryObject obj in geoObjectArray)
                {
                    mesh = obj as Mesh;
                    if (null != mesh)
                    {
                        break;
                    }
                }
                if (null != mesh)
                {
                    //create a PLY by naterial class
                    LuxExporter.PLY.PLY_By_Material PLYWorker = new PLY.PLY_By_Material();
                    
                    //get the mesh material
                    PLYWorker.MaterialID = mesh.MaterialElementId;
                    Material FaceMaterial;
                    FaceMaterial = vDoc.get_Element(PLYWorker.MaterialID) as Material;

                    if (FaceMaterial == null)
                    {
                        PLYWorker.MaterialName = "Default";
                    }
                    else
                    {
                        PLYWorker.MaterialName = FaceMaterial.Name.ToString();
                        //in future: check whether this is a Glass2 Material via stored data
                        //for now just check transparency
                        if (FaceMaterial.Transparency > 0)
                        {
                            PLYWorker.Is_Glass2 = true;
                        }
                    }
                                        
                    //export mesh
                    PLYWorker.TriangulateTopoMesh(mesh);
                    //create output filr path
                    String sOutputPathSceneFile = vOutputFilePath + vRevitElement.UniqueId.ToString() + "_0_0.PLY";
                    
                    //write ply file out:
                    WritePLYFile(PLYWorker, sOutputPathSceneFile);
                    
                    //create geo file string
                    strMesh.AppendLine("AttributeBegin  # " + vRevitElement.Name.ToString());
                    strMesh.AppendLine("NamedMaterial \"" + PLYWorker.MaterialName + "\"");
                    strMesh.AppendLine("");
                    if (PLYWorker.Is_Glass2)
                    {
                        strMesh.AppendLine("Interior \""+PLYWorker.MaterialName+"\"");
                        //strMesh.AppendLine("Exterior  \"World\"");
                    }
                    strMesh.AppendLine("Shape \"plymesh\"");
                    strMesh.AppendLine("\"string filename\" [\"" + sOutputPathSceneFile + "\"]");
                    strMesh.AppendLine("\"string subdivscheme\" [\"loop\"]");
                    strMesh.AppendLine("\"integer nsubdivlevels\" [0]");
                    strMesh.AppendLine("\"bool dmnormalsmooth\" [\"false\"]");
                    strMesh.AppendLine("\"bool dmsharpboundary\" [\"true\"]");
                    strMesh.AppendLine("\"bool dmnormalsplit\" [\"false\"]");
                    strMesh.AppendLine("AttributeEnd #  \"\"");
                }
                else
                {
                    //no valid mesh found
                    strMesh = null;
                }

                //return geo file info
                return strMesh;

            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in module Revit_Geometry_Worker (ExportToPLY_Mesh)");
                return null;
                throw;
            }
            


        }
        public StringBuilder ExportToPLY_Solid(Options GeometryOption,Boolean Transform)
        {
            try
            {
                //get geometry element from element
                Autodesk.Revit.DB.GeometryElement GeoElement = vRevitElement.get_Geometry(GeometryOption);
                
                //store the unique ID and name
                strObjectName = new StringBuilder(vRevitElement.Id.ToString() + "_" + vRevitElement.Name.ToString());
                
                //setup export string
                StringBuilder strExport = new StringBuilder("");

                //initiate object Counter for file name
                //Int32 ObjectCounter = 0;
                
                //make sure we are only trying to export objects with geo information
                if (GeoElement!=null)
                {
                    //loop through all geometry objects in element
                    foreach (GeometryObject item in GeoElement.Objects)
                    {
                        if (item.GetType()==typeof(GeometryInstance))
                        {
                            // Get the geometry instance which contains the geometry information
                            Autodesk.Revit.DB.GeometryInstance instance = item as Autodesk.Revit.DB.GeometryInstance;
                            //start digging down into nested items...
                            InstanceIterator(item as GeometryInstance,Transform, vObjectCounter, strExport).ToString();
                        }
                        else
	                    {
                            if (item.GetType()==typeof(Solid))
                            {
                                //handle non instances-> ie walls , columns ...
                                Autodesk.Revit.DB.Solid solid = item as Autodesk.Revit.DB.Solid;
                                //check whether we have a valid solid
                                if (null == solid || 0 == solid.Faces.Size || 0 == solid.Edges.Size)
                                {
                                    continue;
                                }
                                //get export string
                                strExport.AppendLine(ExportSolid(solid, null, vObjectCounter.ToString()).ToString());
                                //increase object counters ???
                                vObjectCounter++;
                            }
	                    }
                    }
                }
                if (strExport.Length > 0)
                {
                    //reset object counter
                    vObjectCounter = 0;
                    //return export string
                    return strExport;
                }
                else
                {
                    //reset object counter
                    vObjectCounter = 0;
                    //nothing to return
                    return null;
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in module Revit_Geometry_Worker (ExportToPLY_Solid)");
                throw;
            }
        }

       
        private StringBuilder InstanceIterator(Autodesk.Revit.DB.GeometryInstance instance,Boolean T, Int32 ObjectCounter,StringBuilder strExport)
        {
            try
            {
                foreach (GeometryObject objgetest in instance.SymbolGeometry.Objects)
                {
                    //get the graphic style
                    GraphicsStyle gStyle = vDoc.get_Element(instance.GraphicsStyleId) as GraphicsStyle;
                    //check for light source
                    if (gStyle != null)
                    {
                        if (gStyle.GraphicsStyleCategory.Name == "Light Source")
                        {
                            //this is a light source ... skip this object
                            continue;
                        }
                    }
                    //check object type if solid export if instance digg deeper...
                    if (objgetest.GetType() == typeof(Solid))
                    {
                        //check whether we have a valid solid
                        Autodesk.Revit.DB.Solid solid = objgetest as Autodesk.Revit.DB.Solid;
                        if (null == solid || 0 == solid.Faces.Size || 0 == solid.Edges.Size)
                        {
                            continue;
                        }
                        
                        //get transformation if required
                        Transform instTransform;
                        //if (T)
                        //{
                        //    instTransform = instance.Transform;    
                        //}
                        //else
                        //{
                            instTransform = null;
                        //}
                        
                        //get the solid exported
                        strExport.Append(ExportSolid(solid, instTransform, vObjectCounter.ToString()).ToString());
                        //increase object counters
                        vObjectCounter++;
                    }
                    else
                    {
                        if (objgetest.GetType() == typeof(GeometryInstance))
                        {
                            //do i need to do anything with transformation here???
                            InstanceIterator(objgetest as GeometryInstance, T, ObjectCounter, strExport);
                        }
                    }
                }

                return strExport;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error in module Revit_Geometry_Worker (Instanceiterator) "+ex.Message);
                throw;
            }
            
        }

        // it requires 
        // an autodesk.revit.db.solid object 
        // a transformation (can be null, only required for non system families)
        //----------------------------------------------------------------
        public StringBuilder ExportSolid(Solid solid,Transform instTransform, String ObjectCounter)
        { 
            //setup utility class
            LuxExporter.UnitConverter Converter = new UnitConverter();
            StringBuilder strSolid = new StringBuilder("");
            //setup point list
            List<String> lPointList = new List<string>();
            //setup points string
            StringBuilder strPoints = new StringBuilder("");
            //setup list for material IDs
            List<String> MaterialIDsList = new List<string>();
            //set up list for PLY by Material data
            List<LuxExporter.PLY.PLY_By_Material> PLYList = new List<LuxExporter.PLY.PLY_By_Material>();

            // Get the faces and edges from solid, and transform the formed points
            foreach (Face face in solid.Faces)
            {
                
                try
                {
                    System.Collections.Generic.IList<Face> FaceList = face.GetRegions();
                    //check whether face has split regions on it
                    if (FaceList.Count>1)
                    {
                        //loop thrpugh all faces
                        foreach (Face item in FaceList)
                        {
                            GetPly(item, MaterialIDsList, PLYList, instTransform);
                            ////get the material ID
                            //String sMaterialID = item.MaterialElementId.ToString();
                            ////check whether we have this material already
                            //if (MaterialIDsList.Contains(sMaterialID))
                            //{
                            //    //get index of PLY
                            //    LuxExporter.PLY.PLY_By_Material PLYWorker = PLYList[MaterialIDsList.IndexOf(sMaterialID)];
                            //    //process face
                            //    PLYWorker.TriangulateFace(item, instTransform);
                            //}
                            //else
                            //{
                            //    MaterialIDsList.Add(sMaterialID);
                            //    LuxExporter.PLY.PLY_By_Material PLYWorker = new PLY.PLY_By_Material();
                            //    //process face
                            //    PLYWorker.TriangulateFace(item, instTransform);
                            //    PLYWorker.MaterialID = item.MaterialElementId;
                            //    Material FaceMaterial;
                            //    FaceMaterial= vDoc.get_Element(PLYWorker.MaterialID)as Material;

                            //    if (FaceMaterial == null)
                            //    {
                            //        PLYWorker.MaterialName = "Default";
                            //    }
                            //    else
                            //    {
                            //        PLYWorker.MaterialName = FaceMaterial.Name.ToString();
                            //        //in future: check whether this is a Glass2 Material via stored data
                            //        //for now just check transparency
                            //        if (FaceMaterial.Transparency > 0)
                            //        {
                            //            PLYWorker.Is_Glass2 = true;
                            //        }
                            //    }
                            //    //add to list
                            //    PLYList.Add(PLYWorker);
                            //}
                        }
                    }
                    else
                    {
                        GetPly(face, MaterialIDsList, PLYList, instTransform);
                        ////get the material ID
                        //String sMaterialID = face.MaterialElementId.ToString();

                        ////check whether we have this material already
                        //if (MaterialIDsList.Contains(sMaterialID))
                        //{
                        //    //get index of PLY
                        //    LuxExporter.PLY.PLY_By_Material PLYWorker = PLYList[MaterialIDsList.IndexOf(sMaterialID)];
                        //    //process face
                        //    PLYWorker.TriangulateFace(face, instTransform);
                        //}
                        //else
                        //{
                        //    MaterialIDsList.Add(sMaterialID);
                        //    LuxExporter.PLY.PLY_By_Material PLYWorker = new PLY.PLY_By_Material();
                        //    //process face
                        //    PLYWorker.TriangulateFace(face, instTransform);
                        //    PLYWorker.MaterialID = face.MaterialElementId;
                        //    Material FaceMaterial;
                        //    FaceMaterial = vDoc.get_Element(PLYWorker.MaterialID) as Material;

                        //    if (FaceMaterial == null)
                        //    {
                        //        PLYWorker.MaterialName = "Default";
                        //    }
                        //    else
                        //    {
                        //        PLYWorker.MaterialName = FaceMaterial.Name.ToString();
                        //        //in future: check whether this is a Glass2 Material via stored data
                        //        //for now just check transparency
                        //        if (FaceMaterial.Transparency > 0)
                        //        {
                        //            PLYWorker.Is_Glass2 = true;
                        //        }
                        //    }

                        //    //add to list
                        //    PLYList.Add(PLYWorker);

                       // }
                    }
                 }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Error in module Revit_Geometry_Worker (ExportSolid::FACES Loop) "+ex.Message);
                    return strSolid;
                }
            }

            //set up counter for ply files of the same revit element
            Int16 iPLYCounter = 0;
            try
            {
                //need to create a ply file per material!
                //create PLY folder

            foreach (LuxExporter.PLY.PLY_By_Material item in PLYList)
            {
                //create output filr path
                String sOutputPathSceneFile = vOutputFilePath + vRevitElement.UniqueId.ToString() + "_" + ObjectCounter + "_" + iPLYCounter.ToString() + ".PLY";
                //write file
                WritePLYFile(item, sOutputPathSceneFile);

                //create geo file string
                strSolid.AppendLine("AttributeBegin  # " + vRevitElement.Name.ToString());
                strSolid.AppendLine("NamedMaterial \"" + item.MaterialName + "\"");
                if (item.Is_Glass2)
                {
                    strSolid.AppendLine("Interior \"" + item.MaterialName + "\"");
                    //strSolid.AppendLine("Exterior  \"World\"");
                }
                strSolid.AppendLine("");
                strSolid.AppendLine("Shape \"plymesh\"");
                strSolid.AppendLine("\"string filename\" [\"" + sOutputPathSceneFile + "\"]");
                strSolid.AppendLine("\"string subdivscheme\" [\"loop\"]");
                strSolid.AppendLine("\"integer nsubdivlevels\" [0]");
                strSolid.AppendLine("\"bool dmnormalsmooth\" [\"false\"]");
                strSolid.AppendLine("\"bool dmsharpboundary\" [\"true\"]");
                strSolid.AppendLine("\"bool dmnormalsplit\" [\"false\"]");
                strSolid.AppendLine("AttributeEnd #  \"\"");

                 // increase counter
                iPLYCounter ++;
            }

            //return Mesh data
            return strSolid;

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error in module Revit_Geometry_Worker (ExportSolid::Writing PLY files Loop) "+ex.Message);
                throw;
            }
            
        }

        #endregion

        #region Helper Functions

        private void GetPly(Face item, List<String> MaterialIDsList, List<LuxExporter.PLY.PLY_By_Material> PLYList, Transform instTransform)
        {

            //get the material ID
            String sMaterialID = item.MaterialElementId.ToString();
            //check whether we have this material already
            if (MaterialIDsList.Contains(sMaterialID))
            {
                //get index of PLY
                LuxExporter.PLY.PLY_By_Material PLYWorker = PLYList[MaterialIDsList.IndexOf(sMaterialID)];
                //process face
                PLYWorker.TriangulateFace(item, instTransform);
            }
            else
            {
                MaterialIDsList.Add(sMaterialID);
                LuxExporter.PLY.PLY_By_Material PLYWorker = new PLY.PLY_By_Material();
                //process face
                PLYWorker.TriangulateFace(item, instTransform);
                PLYWorker.MaterialID = item.MaterialElementId;
                Material FaceMaterial;
                FaceMaterial = vDoc.get_Element(PLYWorker.MaterialID) as Material;

                if (FaceMaterial == null)
                {
                    PLYWorker.MaterialName = "Default";
                }
                else
                {
                    PLYWorker.MaterialName = FaceMaterial.Name.ToString();
                    //in future: check whether this is a Glass2 Material via stored data
                    //for now just check transparency
                    if (FaceMaterial.Transparency > 0)
                    {
                        PLYWorker.Is_Glass2 = true;
                    }
                }
                //add to list
                PLYList.Add(PLYWorker);

            }
        }

        private Boolean WritePLYFile(LuxExporter.PLY.PLY_By_Material item, String sOutputPathSceneFile)
        {
            try
            {
                //get lists
                List<String> VerticesIndexList = item.GetIndecisList();
                List<String> VerticesList = item.GetVerticesList();
                
                //check whether file exists
                System.IO.FileInfo finfo = new System.IO.FileInfo(sOutputPathSceneFile);
                if (finfo.Exists)
                {
                    System.Windows.Forms.MessageBox.Show("WritePLYFile::Element id: " + vRevitElement.Id.ToString()+" OutPut:"+sOutputPathSceneFile);
                    //         throw new System.InvalidOperationException("FileExists"); ;
                }

                //write list out to geometry file
                using (System.IO.StreamWriter PLYFile = new System.IO.StreamWriter(sOutputPathSceneFile))
                {
                    Int32 iNumberofVertices = item.NoOfVertice();
                    Int32 iNumberofFaces = item.NoofFaces;
                    //write out PLY header
                    string[] lines_Header = {"ply",
                                              "format ascii 1.0",
                                              "comment Created by LuxExporter Revit 0.1 exporter for LuxRender - www.luxrender.net",
                                              "element vertex "+iNumberofVertices.ToString(), //add number of vertices here
                                              "property float x",
                                              "property float y",
                                              "property float z",
                                              "property float nx",
                                              "property float ny",
                                              "property float nz",
                                           //   "property float s",
                                           //   "property float t",
                                              "element face "+iNumberofFaces.ToString(), //add number of faces here
                                              "property list uchar uint vertex_indices",
                                              "end_header"};
                    foreach (String Header in lines_Header)
                    {
                        PLYFile.WriteLine(Header);
                    }
                    //write out index list
                    foreach (String Index in VerticesList)
                    {
                        PLYFile.WriteLine(Index);
                    }
                    //write out index list
                    foreach (String Index in VerticesIndexList)
                    {
                        PLYFile.WriteLine(Index);
                    }

                    //close PLY file
                    PLYFile.Close();
                }
                return true;
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in module Revit_Geometry_Worker (WritePLYFile) ");
                return false;
                
            }
        }
        



        #endregion

        #region class constructors
        //class constructor (takes Revit Element as argument)
        public Revit_Geometry_Worker(Autodesk.Revit.DB.Element objElement, String sSceneFilepath,Document cDoc)
        {
            try
            {
                vRevitElement = objElement;
                vOutputFilePath = sSceneFilepath;
                vDoc = cDoc;
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in module Revit_Geometry_Worker (Class Constructor)");
                throw;
            }
            
        }
        #endregion
    }
}
