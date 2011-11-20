using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using Autodesk.Revit.DB;

namespace LuxExporter
{
    class LuxExporter_Main
    {
        #region Properties
    
        //properties:

        //private String strMatchName = "";

        //output file path
        private String vOutputFilePath = "";
        public String OutputFilePath {
            get { return vOutputFilePath; }
            set { vOutputFilePath = value; }
        }

        //list of views to be exported
        private List<Autodesk.Revit.DB.View3D> v3DViewToExport;
        
        public List<Autodesk.Revit.DB.View3D> ViewToExport {
            get { return v3DViewToExport;}
            set{v3DViewToExport = value; }
        }


        //current Revit Document
        private Document vDoc;
        public Document CurrentRevitDocument 
        {
            get { return vDoc; }
            set { vDoc = value; }
        }
        
        //linked files in Document
        private DocumentSet vLinkedFiles;
        public DocumentSet LinkedFiles
        {
            get { return vLinkedFiles; }
            set { vLinkedFiles = value; }
        }

        private Transform RevitLinkTransform;

        #endregion 

        #region Main Functions
        // main function
        public void ExportToLux()
        {

            
           
            //loop through views and export to lux
            foreach (Autodesk.Revit.DB.View3D ExportView in v3DViewToExport)
            {

                // Create new stopwatch
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //set up list geoFile to include
                List<String>GeoFileList = new List<string>();

                //get all elements in view 
                FilteredElementCollector viewCollector = new FilteredElementCollector(vDoc, ExportView.Id);
                
                //i could add a filter for each category here ( or for specials like lights...)
                //viewCollector.OfCategory(BuiltInCategory.OST_Walls);
                
                //cast views elements to list
                IEnumerable<Element> ElementList = viewCollector.Cast<Element>();
                
                //create scene folder
                //check whether folder allready exists
                //if not create scene folder
                //create PLY folder
                //create RES folder (textures & other data)
                if (Directory.Exists(vOutputFilePath + ExportView.Name.ToString()))
                {
                    //delete folder??

                }
                else
                {
                    //create all directories
                    //make sure view name has no illegal characters: {}
                    //create scene Directory
                    Directory.CreateDirectory(vOutputFilePath + ExportView.Name.ToString());
                    //create PLY folder
                    Directory.CreateDirectory(vOutputFilePath + ExportView.Name.ToString() + "/" + "PLY");
                    //create resources folder
                    Directory.CreateDirectory(vOutputFilePath + ExportView.Name.ToString() + "/" + "RES");
                }

                //set up geofile path variable
                String sOutputPathGeoFile = "";
                //setup PLY path variable
                String PLYDirectory = "";
                //check for linked Revit Files visible in view
                LuxExporter.Revit_Filter Filter = new Revit_Filter(vDoc);
                List<Element> RevitLinks = Filter.GetRevitLinks(ExportView.Id);

                //setup the geomtry option for the current view
                Autodesk.Revit.DB.Options GeometryOption = new Options();
                GeometryOption.ComputeReferences = true;
                GeometryOption.View = ExportView;

                //export linked files
                if (RevitLinks.Count > 0)
                {

                    //count how many instances of each individual link exist
                    //check whether any of these instances collides with section box if there is any
                    //if collision no instance, if no collision or no section box instanciate whole file 
                    //export link file and instanciate as often as required


                    //create dictionary storing linked file name and number of occurences
                    Dictionary<String, int> DRevitLinks = new Dictionary<string, int>();

                    //list containing all links of models not cut by section box and their transformation data
                    List<LuxExporter.Revit_Linked_Files> lWholeModelLinks = new List<Revit_Linked_Files>();
                    //list containing all links of models cut by section box and their transformation data
                    List<LuxExporter.Revit_Linked_Files> lCutModelLinks = new List<Revit_Linked_Files>();

                    int LinkCounter = 0;
                    //loop through link list and sort items before exporting
                    foreach (Element LinkItem in RevitLinks)
                    {
                        //increase link Counter
                        LinkCounter++;
                        //remove stuff from name
                        String LinkName = LinkItem.Name.ToString().Substring(0, LinkItem.Name.ToString().IndexOf(".rvt"));
                        //flag for bounding box check
                        Boolean BoundingBoxOK = true;
                        //check whether bounding box active
                        //pointer
                        int NumberOfLinkOccurences = 0;

                        if (ExportView.SectionBox.Enabled)
                        {
                            //if yes does link clash with box?
                            //get boundingbox of item
                            BoundingBoxXYZ ElementBounding = LinkItem.get_BoundingBox(ExportView);
                            //get sectionbox
                            Autodesk.Revit.DB.BoundingBoxXYZ ViewSectionBox = ExportView.SectionBox;
                            //check whether element bounding box is completely enclosed in view bounding box if not disable instancing!
                            LuxExporter.Revit_BoundingBox_Checker checker = new Revit_BoundingBox_Checker();
                            BoundingBoxOK = checker.BoundingBox_Checker(ElementBounding, ViewSectionBox);
                        }

                        //get the link transformation
                        Instance inst = LinkItem as Instance;
                        RevitLinkTransform = inst.GetTransform();

                        if (BoundingBoxOK)
                        {
                            //if no boundingbox and no clash
                            //check whether link already in list
                            if (lWholeModelLinks.Contains(new LuxExporter.Revit_Linked_Files(LinkName)))
                            {
                                //get link class and add transformation
                                LuxExporter.Revit_Linked_Files dummyLink = new Revit_Linked_Files(LinkName);
                                LuxExporter.Revit_Linked_Files ExLink = lWholeModelLinks.Find(xy => xy.LinkName == dummyLink.LinkName);
                                ExLink.AddTransForm(RevitLinkTransform);

                            }
                            else
                            {
                                //create new link class
                                LuxExporter.Revit_Linked_Files dummyLink = new Revit_Linked_Files(LinkName);
                                //add transformation
                                dummyLink.AddTransForm(RevitLinkTransform);
                                //add link element
                                dummyLink.RevitLink = LinkItem;
                                //add link short name
                                dummyLink.LinkShortName = LinkCounter.ToString();
                                //check if this link has been exported before
                                if (DRevitLinks.TryGetValue(LinkName, out NumberOfLinkOccurences))
                                {
                                    //increase counter
                                    DRevitLinks[LinkName] = NumberOfLinkOccurences + 1;
                                    NumberOfLinkOccurences++;

                                }
                                else
                                {
                                    //add link to dictionary
                                    DRevitLinks.Add(LinkName, 0);
                                }
                                //remove .rvt from link instance name 
                                PLYDirectory = vOutputFilePath + ExportView.Name.ToString() + "/PLY/" + LinkName + "_" + NumberOfLinkOccurences.ToString() + "/";
                                
                                //store unique LinkName in class
                                dummyLink.UniqueLinkName = LinkName + "_" + NumberOfLinkOccurences.ToString();
                                //store path in class
                                dummyLink.PLYFolderPath = PLYDirectory;
                                //create geometry file name
                                String GeoFilePath = vOutputFilePath + ExportView.Name.ToString() + "/" + LinkName + "_" + NumberOfLinkOccurences.ToString() + "-geom.lxo";
                                dummyLink.GeoFilePath = GeoFilePath;
                                //add to list
                                lWholeModelLinks.Add(dummyLink);
                            }
                        }
                        else
                        {
                            //file need to be exported again
                            //create new link class
                            LuxExporter.Revit_Linked_Files dummyLink = new Revit_Linked_Files(LinkName);
                            //add transformation
                            dummyLink.AddTransForm(RevitLinkTransform);
                            //add link element
                            dummyLink.RevitLink = LinkItem;
                            //add link short name
                            dummyLink.LinkShortName = LinkCounter.ToString();
                            //check if this link has been exported before
                            if (DRevitLinks.TryGetValue(LinkName, out NumberOfLinkOccurences))
                            {
                                //increase counter
                                DRevitLinks[LinkName] = NumberOfLinkOccurences + 1;
                                NumberOfLinkOccurences++;

                            }
                            else
                            {
                                //add link to dictionary
                                DRevitLinks.Add(LinkName, 0);
                            }
                            //remove .rvt from link instance name 
                            PLYDirectory = vOutputFilePath + ExportView.Name.ToString() + "/PLY/" + LinkName + "_" + NumberOfLinkOccurences.ToString() + "/";
                            //store unique LinkName in class
                            dummyLink.UniqueLinkName = LinkName + "_" + NumberOfLinkOccurences.ToString();
                            //store path in class
                            dummyLink.PLYFolderPath = PLYDirectory;
                            //create geometry file name
                            String GeoFilePath = vOutputFilePath + ExportView.Name.ToString() + "/" + LinkName + "_" + NumberOfLinkOccurences.ToString() + "-geom.lxo";
                            dummyLink.GeoFilePath = GeoFilePath;
                            //add to list
                            lCutModelLinks.Add(dummyLink);
                        }

                    }

                    //combine linked model lists into one list
                    List<LuxExporter.Revit_Linked_Files> lModelsCombined = new List<Revit_Linked_Files>();
                    lModelsCombined.AddRange(lWholeModelLinks.ToArray());
                    lModelsCombined.AddRange(lCutModelLinks.ToArray());

                    try
                    {
                        //loop through all model links and export them
                        //foreach (LuxExporter.Revit_Linked_Files item in lWholeModelLinks)
                        foreach (LuxExporter.Revit_Linked_Files item in lModelsCombined)
                        {
                            //create PLY output directory
                            Directory.CreateDirectory(item.PLYFolderPath);
                            //get elements in link
                            foreach (Document ditem in vLinkedFiles)
                            {
                                //check for match in name
                                if (ditem.PathName.Contains(item.LinkName))
                                {
                                    //export materials

                                    if (FilteredElementCollector.IsViewValidForElementIteration(ditem,ExportView.Id))
                                    {
                                        //get all elements in view 
                                        FilteredElementCollector LinkViewCollector = new FilteredElementCollector(ditem, ExportView.Id);

                                        //cast views elements to list
                                        IEnumerable<Element> LinkElementList = LinkViewCollector.Cast<Element>();
                                        //export link
                                        GeoFileList.Add(ExportElementList(LinkElementList, ExportView, GeometryOption, item.GeoFilePath, item.PLYFolderPath, item));    
                                    }
                                    else
                                    {
                                        //get model element filter
                                        LuxExporter.Revit_Filter FilterUtility = new Revit_Filter();
                                        //this filter also select non placed items!!! 
                                        Autodesk.Revit.DB.LogicalOrFilter ModelFilter = FilterUtility.BuildFullModelFilter();
                                        //filter elements from linked file
                                        Autodesk.Revit.DB.FilteredElementCollector collector = new FilteredElementCollector(ditem);
                                        //filter out types 
                                        collector.WherePasses(ModelFilter).WhereElementIsNotElementType();
                                        //pass eklement list
                                        IEnumerable<Element> LinkElementList =collector.Cast<Element>();
                                        //export link
                                        GeoFileList.Add(ExportElementList(LinkElementList, ExportView, GeometryOption, item.GeoFilePath, item.PLYFolderPath, item));
                                    }
                                    
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Test"+ex.Message);
                        throw;
                    }
                }

                //reset the link transform
                RevitLinkTransform = null;

                sOutputPathGeoFile = vOutputFilePath + ExportView.Name.ToString() + "/" + ExportView.ViewName.ToString() + "-geom.lxo";
                PLYDirectory = vOutputFilePath + ExportView.Name.ToString() + "/PLY/";
                //export host file
                GeoFileList.Add(ExportElementList(ElementList, ExportView, GeometryOption, sOutputPathGeoFile,PLYDirectory,null));
                
                // Stop timing
                stopwatch.Stop();
                MessageBox.Show("Time elapsed: {0}" + stopwatch.Elapsed);


                //create material outputfilepath
                String sOutputPathMatFile = vOutputFilePath + ExportView.Name.ToString() + "/" + ExportView.ViewName.ToString() + "-mat.lxm";

                try
                {
                    //global::System.Windows.Forms.MessageBox.Show("Test");
                    //write out materials file
                    //get materials in view only -->> does not work!
                    //MaterialSet LMaterials =  ExportView.Materials;
                    //get materials in project
                    LuxExporter.Revit_Filter MaterialFilter = new Revit_Filter(vDoc);
                    List<Material> LMaterials = MaterialFilter.GetAllMaterials();

                    
                    //setup helper class
                    LuxExporter.Revit_Material_Worker MaterialExporter = new Revit_Material_Worker(vDoc);
                    //setup export String
                    StringBuilder MaterialExport = new StringBuilder("# Lux Render CVS - Material File");
                    MaterialExport.AppendLine("# Exported by LuxRev 0.2- ALPHA");
                    MaterialExport.AppendLine("# View Name: " + ExportView.ViewName.ToString());
                    MaterialExport.AppendLine("");

                    //add glass2 presets:
                    //World
                    MaterialExport.AppendLine("MakeNamedVolume \"World\" \"homogeneous\"");
	                MaterialExport.AppendLine("\"float fresnel\" [1.000292658805847]");
	                MaterialExport.AppendLine("\"color g\" [-0.30000001 -0.30000001 -0.30000001]");
	                MaterialExport.AppendLine("\"color sigma_a\" [0.00000000 0.00000000 0.00000000]");
                    MaterialExport.AppendLine("\"color sigma_s\" [0.02500000 0.02500000 0.02500000]");
                    //Glass
                    MaterialExport.AppendLine("MakeNamedVolume \"Glass\" \"clear\"");
	                MaterialExport.AppendLine("\"float fresnel\" [1.519999980926514]");
                    MaterialExport.AppendLine("\"color absorption\" [0.01614268 0.00300774 0.03046782]");
                    
                    //loop through all materials and export
                    foreach (Material RevitMaterial in LMaterials)
                    {
                        MaterialExport.Append(MaterialExporter.ExportMaterial(RevitMaterial));
                    }

                    
                    //write out materials file
                    using (System.IO.StreamWriter MatFile = new System.IO.StreamWriter(sOutputPathMatFile))
                    {
                        MatFile.WriteLine(MaterialExport);
                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("Failed To write mat file");
                    throw;
                }

                
                //view utility class
                LuxExporter.Revit_View_ThreeD ViewData = new Revit_View_ThreeD(ExportView);

                //get the camera data
                XYZ CameraPosition = ViewData.CameraPosition;
                XYZ CameraViewDirection = ViewData.CameraViewDirection;
                XYZ CameraUp = ViewData.CameraUp;
                
                //get the screenwindow date
                //assume resolution of 800 x 600
                //lens shift is 0 for x and y
                //scale as per view
                double dScreenaspectRatio = (800.00 / 600.00);
                LuxExporter.LuxExporter_ScreenWindow ScreenWindow = new LuxExporter_ScreenWindow(dScreenaspectRatio, 0, 0, ExportView.Scale);
                Double[] vScreenWindow = ScreenWindow.GetScreenWindow();
 
                //create string for header file - mainly hardcoded as of yet
                StringBuilder LXS = new StringBuilder("# Lux Render CVS - Scene File");
                LXS.AppendLine("# Exported by LuxRev 0.2- ALPHA");
                LXS.AppendLine("# View Name: "+ ExportView.ViewName.ToString());
                LXS.AppendLine("LookAt "+CameraPosition.X.ToString("f6")+" "+CameraPosition.Y.ToString("f6")+" "+CameraPosition.Z.ToString("f6"));
                LXS.AppendLine("  " + CameraViewDirection.X.ToString("f6") + " " + CameraViewDirection.Y.ToString("f6") + " " + CameraViewDirection.Z.ToString("f6"));
                LXS.AppendLine("  "+CameraUp.X.ToString("f6")+" "+CameraUp.Y.ToString("f6")+" "+CameraUp.Z.ToString("f6"));
                LXS.AppendLine("");                    
                LXS.AppendLine("Camera \"perspective\"");
                LXS.AppendLine("  \"float fov\" [150]");
                LXS.AppendLine("  \"float hither\" [0]");
                LXS.AppendLine("  \"float yon\" [10000000]");
                LXS.AppendLine("  \"float lensradius\" [0]");
                LXS.AppendLine("  \"bool autofocus\" [\"true\"]");
                LXS.AppendLine("  \"float shutteropen\" [0]");
                LXS.AppendLine("  \"float shutterclose\" [1]");
                LXS.AppendLine( "  \"float screenwindow\" ["+vScreenWindow[0]+" "+vScreenWindow[1]+" "+vScreenWindow[2]+" "+vScreenWindow[3]+"]");
                LXS.AppendLine("");
                LXS.AppendLine("Film \"fleximage\"");
                LXS.AppendLine("  \"integer xresolution\" [800]");
                LXS.AppendLine("  \"integer yresolution\" [600]");
                LXS.AppendLine("  \"integer haltspp\" [0]");
                LXS.AppendLine("  \"bool premultiplyalpha\" [\"false\"]");
                LXS.AppendLine("  \"string tonemapkernel\" [\"reinhard\"]");
                LXS.AppendLine("  \"float reinhard_postscale\" [1.200000]");
                LXS.AppendLine("  \"float reinhard_burn\" [6.000000]");
                LXS.AppendLine("  \"integer displayinterval\" [8]");
                LXS.AppendLine("  \"integer writeinterval\" [120]");
                LXS.AppendLine("  \"string ldr_clamp_method\" [\"lum\"]");
                LXS.AppendLine("  \"bool write_exr\" [\"false\"]");
                LXS.AppendLine("  \"bool write_png\" [\"true\"]");
                LXS.AppendLine("  \"string write_png_channels\" [\"RGB\"]");
                LXS.AppendLine("  \"bool write_png_16bit\" [\"false\"]");
                LXS.AppendLine("  \"bool write_png_gamutclamp\" [\"true\"]");
                LXS.AppendLine("  \"bool write_tga\" [\"false\"]");
                LXS.AppendLine("  \"string filename\" [\"C:\\\\temp\\\\lux\\\\"+ExportView.ViewName.ToString()+"\"]");
                LXS.AppendLine("  \"bool write_resume_flm\" [\"false\"]");
                LXS.AppendLine("  \"bool restart_resume_flm\" [\"true\"]");
                LXS.AppendLine("  \"integer reject_warmup\" [128]");
                LXS.AppendLine("  \"bool debug\" [\"false\"]");
                LXS.AppendLine("  \"float colorspace_white\" [0.314275 0.329411]");
                LXS.AppendLine("  \"float colorspace_red\" [0.630000 0.340000]");
                LXS.AppendLine("  \"float colorspace_green\" [0.310000 0.595000]");
                LXS.AppendLine("  \"float colorspace_blue\" [0.155000 0.070000]");
                LXS.AppendLine("  \"float gamma\" [2.2]");
                LXS.AppendLine("");                 
                LXS.AppendLine("PixelFilter \"mitchell\"");
                LXS.AppendLine("  \"float B\" [0.75]");
                LXS.AppendLine("  \"float C\" [0.125]");
                LXS.AppendLine(""); 
                LXS.AppendLine("Sampler \"metropolis\"");
                LXS.AppendLine("  \"float largemutationprob\" [0.4]");
                LXS.AppendLine("");
                LXS.AppendLine("SurfaceIntegrator \"bidirectional\"");
                LXS.AppendLine("  \"integer eyedepth\" [48]");
                LXS.AppendLine("  \"integer lightdepth\" [48]");
                LXS.AppendLine(""); 
                LXS.AppendLine("VolumeIntegrator \"single\"");
                LXS.AppendLine("  \"float stepsize\" [1]");
                LXS.AppendLine("Accelerator \"tabreckdtree\"");
                LXS.AppendLine("  \"integer intersectcost\" [80]");
                LXS.AppendLine("  \"integer traversalcost\" [1]");
                LXS.AppendLine("  \"float emptybonus\" [0.2]");
                LXS.AppendLine("  \"integer maxprims\" [1]");
                LXS.AppendLine("  \"integer maxdepth\" [-1]");
                LXS.AppendLine("");              
                LXS.AppendLine("WorldBegin");
                LXS.AppendLine(""); 
                LXS.AppendLine("AttributeBegin");
                LXS.AppendLine("LightGroup \"default\"");
                LXS.AppendLine("  LightSource \"sunsky\"");
                LXS.AppendLine("AttributeEnd");
                LXS.AppendLine("");
                
                LXS.AppendLine("Include \""+sOutputPathMatFile+"\"");
                LXS.AppendLine("");

                foreach (String FilePath in GeoFileList)
                {
                    LXS.AppendLine("Include \"" + FilePath + "\"");
                }
                
                LXS.AppendLine("");                 
                LXS.AppendLine("WorldEnd");
                                 
                //create outputfilepath
                String sOutputPathSceneFile = vOutputFilePath + ExportView.Name.ToString()+"/"+ ExportView.ViewName.ToString() + ".lxs";

                //write out Scene file
                using (System.IO.StreamWriter LXSFile = new System.IO.StreamWriter(sOutputPathSceneFile))
                {
                    LXSFile.WriteLine(LXS);
                }
            }
        }



        private String ExportElementList(IEnumerable<Element> ElementList, 
            View3D ExportView, Autodesk.Revit.DB.Options GeometryOption, 
            String sOutputPathGeoFile,
            String sPLYFilePath,
            LuxExporter.Revit_Linked_Files RevitLink)
        {
            
            //set up export string list
            List<StringBuilder> ListExport = new List<StringBuilder>();

            //setup list containing topo information
            List<Autodesk.Revit.DB.TopographySurface> lTopos = new List<TopographySurface>();

            //setup list containing Lights
            List<FamilyInstance> Lights = new List<FamilyInstance>();

            //setup list containing instance information
            List<LuxExporter.Revit_Instance> lFamilyInstances = new List<Revit_Instance>();

            List<String> LDebug = new List<string>();

            //loop through all elements and export
            foreach (Element item in ElementList)
            {
                //check when link if item is visible in view
                if (!IsHiddenElementOrCategory(item,ExportView))
                {
                    
                 
                    //debug
                    LDebug.Add(item.Id.ToString() + " " + item.GetType().ToString());
                
                    //check for topo surface and add to collection if true for later processing
                    if (item.GetType() == typeof(Autodesk.Revit.DB.TopographySurface))
                    {
                        lTopos.Add(item as Autodesk.Revit.DB.TopographySurface);
                        //jump to next item
                        continue;
                    }
                    
                    //check whether we have a link file and add link identifier to object name
                    String LinkName = "";
                    if (RevitLink!=null)
                    {
                        LinkName = RevitLink.LinkShortName;
                    }
                    else
                    {
                        LinkName = null;
                    }
                    //check whether we can instance the element
                    //create unique family type name
                    LuxExporter.Revit_Instance objInstance = new Revit_Instance(item, vDoc,LinkName);
                    //flag for bounding box check
                    Boolean BoundingBoxOK = true;
                
                    //only if view has sectionbox active!
                    if (ExportView.SectionBox.Enabled)
                    {

                        //get boundingbox of item
                        BoundingBoxXYZ ElementBounding = item.get_BoundingBox(ExportView);
                        //if link I will need to transform the items boundingbox by the link transformatio
                        if (RevitLink!=null)
                        {
                            //get the first transform (models clashing with section box will only have one) 
                            Transform t= RevitLink.Transformations[0];
                            //ElementBounding = new BoundingBoxXYZ(t.OfPoint(ElementBounding.Max, t.OfPoint(ElementBounding.Min)));
                        }
                        
                        //get sectionbox
                        Autodesk.Revit.DB.BoundingBoxXYZ ViewSectionBox = ExportView.SectionBox;
                        //check whether element bounding box is completely enclosed in view bounding box if not disable instancing!
                        LuxExporter.Revit_BoundingBox_Checker checker = new Revit_BoundingBox_Checker();
                        BoundingBoxOK = checker.BoundingBox_Checker(ElementBounding, ViewSectionBox);
                    }

                    //check whether we have a lighting fixture family
                    if (objInstance.CategoryName == "lighting fixtures")
                    {
                        //add family to list
                        Lights.Add(item as Autodesk.Revit.DB.FamilyInstance);
                    }

                    //check whether family contains light sources 
                    List<FamilyInstance> NestedLights = objInstance.GetNestedLightFamilies();
                    if (NestedLights != null)
                    {

                        //add nested families to list
                        foreach (FamilyInstance iLight in NestedLights)
                        {
                            Lights.Add(iLight);
                        }
                    }

                    //check if we have a family vs system family and whether family is completely within section box
                    if (!objInstance.ElementIsSystemFamily && BoundingBoxOK)
                    {
                        //export geometry
                        //get the export string from geometry class 
                        try
                        {
                            StringBuilder dummy = new StringBuilder("");

                            //check whether item is allready in list
                            if (lFamilyInstances.Contains(objInstance))
                            {
                                //instanciate lux object
                                dummy.AppendLine(CreateLuxObjectInstanceFromFamily(objInstance, RevitLink).ToString());
                                //add to output
                                ListExport.Add(dummy);
                            }
                            else
                            {
                                // instanciate worker class and pass geo element to it
                                Revit_Geometry_Worker Exporter = new Revit_Geometry_Worker(item, sPLYFilePath, vDoc);
                                //get geo data without transformation
                                StringBuilder ExportString = Exporter.ExportToPLY_Solid(GeometryOption, false);

                                //need to check whether instance has location point if yes add to list other wise no instancing enabled!!
                                Boolean CheckForInsertionPoint = objInstance.HasInsertionPoint();

                                String sPrefix = "";
                                //check whether this item comes from linked file
                                if (objInstance.HostName!=null)
                                {
                                    sPrefix = objInstance.HostName + "_";
                                }

                                //check whether item has insertion point
                                if (CheckForInsertionPoint)
                                {
                                    //'add the instance to list only if instance has an insertion point rather then a insertion curve
                                    lFamilyInstances.Add(objInstance);

                                    dummy.AppendLine("ObjectBegin " + "\"" + sPrefix + objInstance.GetUniqueFamilyInstanceName.ToString() + "\"");
                                }
                                else
                                {
                                    dummy.AppendLine("ObjectBegin " + "\"" + sPrefix + item.UniqueId.ToString() + "\"");
                                }

                                if (ExportString != null)
                                {
                                    //dummy = CreateLuxAttribute(ExportString, RevitLink, ObjectName);
                                    //dummy.AppendLine("");
                                    ListExport.Add(dummy);
                                    //add ply data to list...
                                    ListExport.Add(ExportString);
                                    //place an instance of the object
                                    dummy = new StringBuilder("");
                                    dummy.AppendLine("ObjectEnd");
                                    dummy.AppendLine("");
                                    dummy.AppendLine(CreateLuxObjectInstanceFromFamily(objInstance, RevitLink).ToString());
                                    
                                    ListExport.Add(dummy);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error while trying to export family instance");
                            throw;
                        }
                    }
                    else
                    //system family 
                    {
                        try
                        {
                            // instanciate worker class and pass geo element to it
                            Revit_Geometry_Worker Exporter = new Revit_Geometry_Worker(item, sPLYFilePath, vDoc);
                            //get geo data
                            StringBuilder ExportString = Exporter.ExportToPLY_Solid(GeometryOption, false);
                            //set up lux string
                            if (ExportString != null)
                            {
                                //ListExport = CreateLuxAttribute(ExportString, RevitLink, item.UniqueId.ToString());
                                StringBuilder dummy = new StringBuilder("");
                                //get export string
                                dummy=CreateLuxAttribute(ExportString, RevitLink, item.UniqueId.ToString());
                                //add to export list
                                ListExport.Add(dummy);
                                
                                if (RevitLink!=null)
                                {
                                    ///if this is a link file add instance stuff
                                    dummy = CreateLuxInstanceFromSystemFamily(RevitLink, item.UniqueId.ToString());
                                    //add to export list
                                    ListExport.Add(dummy);    
                                }
                                
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error while trying to eport system family");
                            throw;
                        }
                    }

	           }
                
                //debugging
                //write out element id
                String sOutputPathDebugFile = "";
                if (RevitLink!=null)
                {
                    sOutputPathDebugFile = vOutputFilePath + "/" + "Debug_" + RevitLink.UniqueLinkName.ToString() + ".txt";
                }
                else
                {
                    sOutputPathDebugFile = vOutputFilePath + "/" + "Debug.txt";
                }
                
                using (System.IO.StreamWriter DebugFile = new System.IO.StreamWriter(sOutputPathDebugFile))
                {
                    foreach (String debug in LDebug)
                    {
                        DebugFile.WriteLine(debug);
                    }
                    
                }
                    
            }


            try
            {
                //process any topography surface if found
                if (lTopos.Count > 0)
                {
                    //loop through all topos found                   
                    foreach (TopographySurface itemT in lTopos)
                    {
                        // instanciate worker class and pass geo element to it
                        Revit_Geometry_Worker Exporter = new Revit_Geometry_Worker(itemT, sPLYFilePath, vDoc);
                        //get geo data
                        StringBuilder ExportString = Exporter.ExportToPLY_Mesh(GeometryOption,RevitLinkTransform);

                        if (ExportString != null)
                        {
                            StringBuilder dummy = new StringBuilder("");
                            //get formatted export string
                            dummy = CreateLuxAttribute(ExportString, RevitLink, itemT.UniqueId.ToString());
                            //add to export list
                            ListExport.Add(dummy);

                            if (RevitLink != null)
                            {
                                //if this is a link file add instance stuff
                                dummy = CreateLuxInstanceFromSystemFamily(RevitLink, itemT.UniqueId.ToString());
                                //add to export list
                                ListExport.Add(dummy);
                            }

                        }

                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to export topo");
                throw;
            }


            try
            {
                //process any lights found
                foreach (FamilyInstance item in Lights)
                {
                    //check whether familyinstance already processed before...


                    //process lights
                    //LuxExporter.Revit_Light_Worker LightProcessor = new Revit_Light_Worker(item, vDoc);
                    //StringBuilder ExportString = LightProcessor.ExportLight(GeometryOption);

                    //if (ExportString != null)
                    //{
                    //    //StringBuilder dummy = new StringBuilder("");
                    //    //dummy = CreateLuxAttribute(ExportString, RevitLink, item.UniqueId.ToString());
                    //    //ListExport.Add(dummy);
                    //    //if (RevitLink != null)
                    //    //{
                    //    //    dummy = CreateLuxInstanceFromNonFamily(RevitLink, item.UniqueId.ToString());
                    //    //    ListExport.Add(dummy);
                    //    //}
                    //}

                }


                //create geo file
                try
                {
                    //write list out to geometry file
                    using (System.IO.StreamWriter GeoFile = new System.IO.StreamWriter(sOutputPathGeoFile))
                    {
                            foreach (StringBuilder line in ListExport)
                            {
                                GeoFile.WriteLine(line);
                            }
                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to write geoFile");
                    throw;
                }

                return sOutputPathGeoFile;
            }
            catch (Exception)
            {
                MessageBox.Show("Failed To Export Lights");
                throw;
            }


           
        }

        #endregion
        #region Helper Functions

        private StringBuilder CreateLuxObjectInstanceFromFamily(LuxExporter.Revit_Instance objInstance, LuxExporter.Revit_Linked_Files RevitLink)
        {
            StringBuilder dummy = new StringBuilder();
            if (RevitLink==null)
            {
                dummy.AppendLine("AttributeBegin");
                dummy.AppendLine("TransformBegin");
                //calculate Transformation matrix
                LuxExporter.Revit_Transform TransFormMatrix = new Revit_Transform();
                //String sTransFormMatrix = TransFormMatrix.TransformationInHost(objInstance.Origin, objInstance.BaseX, objInstance.BaseY, objInstance.BaseZ);
                String sTransFormMatrix = TransFormMatrix.TransformationInHost(objInstance.FamilyTransform);
                //finalise output string
                dummy.AppendLine("#Revit T CreateLuxObjectInstanceFromFamily");
                dummy.AppendLine("Transform [" + sTransFormMatrix + "]");
                dummy.AppendLine("ObjectInstance \"" + objInstance.GetUniqueFamilyInstanceName.ToString() + "\"");
                dummy.AppendLine("TransformEnd");
                dummy.AppendLine("AttributeEnd");
            }

            else
            {
                foreach (Transform item in RevitLink.Transformations)
                {
                    dummy.AppendLine("AttributeBegin");
                    dummy.AppendLine("TransformBegin");

                    //calculate Transformation matrix
                    LuxExporter.Revit_Transform TransFormMatrix = new Revit_Transform();
                    String sTransFormMatrix = "";
                    
                    //if (item.IsTranslation)
                    //{
                    //    dummy.AppendLine("#Translation only");
                    //    sTransFormMatrix = TransFormMatrix.CalculateLuxTransformationMatrixInLinkTranslationOnly(objInstance, item);
                    //    dummy.AppendLine("Transform [" + sTransFormMatrix + "]");

                    //}
                    //else
                    //{
                        sTransFormMatrix = TransFormMatrix.TransformationInLink(objInstance, item);
                        dummy.AppendLine("#Revit T ...");
                        dummy.AppendLine("Transform [" + sTransFormMatrix + "]");
                    //}
                    
                    //finalise output string
                    dummy.AppendLine("ObjectInstance \"" + objInstance.HostName+"_"+ objInstance.GetUniqueFamilyInstanceName.ToString() + "\"");
                    dummy.AppendLine("TransformEnd");
                    dummy.AppendLine("AttributeEnd");
                    dummy.AppendLine("");
                }
            }
                        return dummy;
        }

        private StringBuilder CreateLuxAttribute(StringBuilder ExportString, LuxExporter.Revit_Linked_Files RevitLink, String ObjectName)
        {
            StringBuilder dummy = new StringBuilder("");
            if (RevitLink==null)
            { 
                //add export string
                dummy.AppendLine(ExportString.ToString());
                //add blank line
                dummy.AppendLine("");
            }
            else
            {
                //add object namw
                dummy.AppendLine("ObjectBegin " + "\"" + RevitLink.LinkShortName + "_" + ObjectName + "\"");
                //add export string
                dummy.AppendLine(ExportString.ToString());
                //close object
                dummy.AppendLine("ObjectEnd");
            }

            //return new list
            return dummy; 
        }

        private StringBuilder CreateLuxInstanceFromSystemFamily(LuxExporter.Revit_Linked_Files RevitLink, String ObjectName)
        {
            StringBuilder dummy = new StringBuilder("");

            foreach (Transform item in RevitLink.Transformations)
            {
                //LuxExporter.UnitConverter Converter = new UnitConverter();
                //XYZ transformedOrigin = Converter.ConvertPointCoordToMeter(item.Origin);
                dummy.AppendLine("AttributeBegin");
                dummy.AppendLine("TransformBegin");
                //calculate Transformation matrix
                LuxExporter.Revit_Transform TransFormMatrix = new Revit_Transform();
                //String sTransFormMatrix = TransFormMatrix.TransformationInHost(transformedOrigin, item.Inverse.BasisX, item.Inverse.BasisY, item.Inverse.BasisZ);
                String sTransFormMatrix = TransFormMatrix.TransformationInHost(item);
                //finalise output string
                dummy.AppendLine("#Revit T CreateLuxInstanceFromNonFamily");
                dummy.AppendLine("Transform [" + sTransFormMatrix + "]");
                dummy.AppendLine("ObjectInstance \"" + RevitLink.LinkShortName + "_" + ObjectName + "\"");
                dummy.AppendLine("TransformEnd");
                dummy.AppendLine("AttributeEnd");
                dummy.AppendLine("");
            }
            
            return dummy;

        }

        static bool IsHiddenElementOrCategory(Autodesk.Revit.DB.Element e, View3D v)
        {
            
            try
            {
                bool hidden = e.IsHidden(v);

                if (!hidden)
                {
                    Category cat = e.Category;
                    while (null != cat && !hidden)
                    {
                        hidden = !cat.get_Visible(v);
                        cat = cat.Parent;
                    }
                }
                return hidden;
            }
            catch (Exception ex)
            {
                return true;
               // MessageBox.Show("Test"+ex.Message.ToString());
               // throw;
            }
           
        }
        #endregion
    }
}
