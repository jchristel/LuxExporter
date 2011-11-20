using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace LuxExporter
{
    class Revit_Filter
    {
        #region Properties
        //current document
        private Document vDoc;
        #endregion

        //filter all 3D Views
        public List<View3D> GetThreeDViews()
        {

            try
            {
                List<View3D> ListThreeDViews = new List<View3D>();


                // setup view filter
                FilteredElementCollector ViewCollector = new FilteredElementCollector(vDoc);

                //apply filter to active revit document
                ViewCollector.OfClass(typeof(View3D));

                //cast views elements to list
                IEnumerable<View3D> ViewList = ViewCollector.Cast<View3D>();

                // go through filter and add sheets to list
                foreach (View3D View in ViewList)
                {
                    //RevitSheetCreator.SheetData SheetData = new RevitSheetCreator.SheetData(Style);
                    if (!View.IsTemplate)
                    {
                        ListThreeDViews.Add(View);
                    }
                    
                }

                //return list with sheet views

                return ListThreeDViews;
            }

            catch (Exception)
            {
                return null;

            }
        }


        public List<Element> GetRevitLinks(ElementId ViewID)
        {



            // Find all RevitLink instances in the View by using category filter
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_RvtLinks);
            // Apply the filter to the elements in the active Document and View
            // Use shortcut WhereElementIsNotElementType() to find wall instances only
            FilteredElementCollector collector = new FilteredElementCollector(vDoc,ViewID);
            IList<Element> Links = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            List<Element> ListLinks = new List<Element>();

            foreach (Element e in Links)
            {
                ListLinks.Add(e);
            }
            
            
            

            //// setup Link filter
            //FilteredElementCollector LinkCollector = new FilteredElementCollector(vDoc);
            ////apply filter to active revit document
            //LinkCollector.OfClass(typeof(RevitLinkType));
            ////cast views elements to list
            //IEnumerable<RevitLinkType> LinkList = LinkCollector.Cast<RevitLinkType>();

            //// go through filter and add sheets to list
            //foreach (RevitLinkType Link in LinkList)
            //{
            //    ListLinks.Add(Link);
            //}

            //return list with sheet views

            return ListLinks;

            //List<Element> links = GetElements(BuiltInCategory.OST_RvtLinks, typeof(Instance), revitApp, doc);

        }

        //filter all Materials
        public List<Material> GetAllMaterials()
        {

            try
            {
                List<Material> ListMaterials = new List<Material>();


                // setup view filter
                FilteredElementCollector MaterialCollector = new FilteredElementCollector(vDoc);

                //apply filter to active revit document
                MaterialCollector.OfClass(typeof(Material));

                //cast views elements to list
                IEnumerable<Material> MaterialList = MaterialCollector.Cast<Material>();

                // go through filter and add sheets to list
                foreach (Material Mat in MaterialList)
                {
                    //RevitSheetCreator.SheetData SheetData = new RevitSheetCreator.SheetData(Style);
                    //if (!View.IsTemplate)
                    //{
                        ListMaterials.Add(Mat);
                    //}

                }

                //return list with sheet views

                return ListMaterials;
            }

            catch (Exception)
            {
                return null;

            }
        }

        public Autodesk.Revit.DB.LogicalOrFilter  BuildFullModelFilter() 

        {

        //Define a Category for each of the Revit Default Categories
        //Dim AreaCat As Autodesk.Revit.DB.ElementCategoryFilter = New Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Areas)
        Autodesk.Revit.DB.ElementCategoryFilter CaseworkCat= new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Casework);
        Autodesk.Revit.DB.ElementCategoryFilter CeilingCat= new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Ceilings);
        Autodesk.Revit.DB.ElementCategoryFilter ColumnCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Columns);
        Autodesk.Revit.DB.ElementCategoryFilter pad = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_BuildingPad);
        Autodesk.Revit.DB.ElementCategoryFilter CPCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_CurtainWallPanels);
        Autodesk.Revit.DB.ElementCategoryFilter CSCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Curtain_Systems);
        Autodesk.Revit.DB.ElementCategoryFilter CS2Cat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_CurtaSystem);
        Autodesk.Revit.DB.ElementCategoryFilter CWMCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_CurtainWallMullions);
        Autodesk.Revit.DB.ElementCategoryFilter DrCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Doors);
        Autodesk.Revit.DB.ElementCategoryFilter EEQCat= new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_ElectricalEquipment);
        Autodesk.Revit.DB.ElementCategoryFilter EFixtCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);
        Autodesk.Revit.DB.ElementCategoryFilter EntCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Entourage);
        Autodesk.Revit.DB.ElementCategoryFilter FloorCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Floors);
        Autodesk.Revit.DB.ElementCategoryFilter FurnCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Furniture);
        Autodesk.Revit.DB.ElementCategoryFilter FurnSysCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_FurnitureSystems);
        Autodesk.Revit.DB.ElementCategoryFilter GenModCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_GenericModel);
        Autodesk.Revit.DB.ElementCategoryFilter LightCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_LightingFixtures);
        Autodesk.Revit.DB.ElementCategoryFilter MassCat= new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Mass);
        Autodesk.Revit.DB.ElementCategoryFilter MechCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment);
        Autodesk.Revit.DB.ElementCategoryFilter ParkCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Parking);
        Autodesk.Revit.DB.ElementCategoryFilter PlantCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Planting);
        Autodesk.Revit.DB.ElementCategoryFilter PlumbCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_PlumbingFixtures);
        Autodesk.Revit.DB.ElementCategoryFilter RailPost = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_RailingBalusterRail);
        Autodesk.Revit.DB.ElementCategoryFilter RailCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_StairsRailing);
        Autodesk.Revit.DB.ElementCategoryFilter RampCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Ramps);
        Autodesk.Revit.DB.ElementCategoryFilter RoadCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Roads);
        Autodesk.Revit.DB.ElementCategoryFilter RoofCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Roofs);
        Autodesk.Revit.DB.ElementCategoryFilter SiteCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Site);
        Autodesk.Revit.DB.ElementCategoryFilter SECat= new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_SpecialityEquipment);
        Autodesk.Revit.DB.ElementCategoryFilter StairCat = new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Stairs);
        Autodesk.Revit.DB.ElementCategoryFilter BeamSysCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_StructuralFramingSystem);
        Autodesk.Revit.DB.ElementCategoryFilter StructColumnCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns);
        Autodesk.Revit.DB.ElementCategoryFilter StructFoundCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation);
        Autodesk.Revit.DB.ElementCategoryFilter StructFramingCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
        Autodesk.Revit.DB.ElementCategoryFilter TopoCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Topography);
        Autodesk.Revit.DB.ElementCategoryFilter WallCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Walls);
        Autodesk.Revit.DB.ElementCategoryFilter WinCat=new Autodesk.Revit.DB.ElementCategoryFilter(BuiltInCategory.OST_Windows);

        IList<ElementFilter> list = new List<ElementFilter>() {CaseworkCat,ColumnCat,CPCat, CSCat, CS2Cat,
                                                                                                       CWMCat, DrCat, EEQCat, EFixtCat, EntCat, FloorCat, 
                                                                                                         FurnCat, FurnSysCat, GenModCat, LightCat, pad,
                                                                                                         MassCat, MechCat, ParkCat, PlantCat, PlumbCat, RailCat,RailPost, 
                                                                                                         RampCat, RoadCat, RoofCat,  SiteCat, 
                                                                                                         SECat, StairCat, BeamSysCat, StructColumnCat, StructFoundCat, 
                                                                                                         StructFramingCat, TopoCat, WallCat, WinCat}; 
        try
        {

            //create actual filter
            Autodesk.Revit.DB.LogicalOrFilter FinalFilter  = new Autodesk.Revit.DB.LogicalOrFilter(list);

            return FinalFilter;
        }
        catch ( Exception)
        {   
            return null;
            }
        }

        #region class constructors
        //class constructor expecting the current Revit Document as argument
        public Revit_Filter(Document doc)
        {
            vDoc = doc;
        }

        //class constructor no argument
        public Revit_Filter()
        {
            
        }

        #endregion
    }
}
