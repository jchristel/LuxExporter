using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Diagnostics;


namespace LuxExporter
{
    class Revit_Instance:IEquatable<Revit_Instance>
    {
        #region Class Properties
        private Autodesk.Revit.DB.Element objElement;

        private String vCategoryName = "";
        public String CategoryName
        { 
            get { return vCategoryName; }
        }

        private Boolean bInstanceSupport = false;
        public Boolean ElementHasInstanceSupport 
        {
            get { return bInstanceSupport; }
        }

        private Boolean bSystemFamily = true;
        public Boolean ElementIsSystemFamily
        {
            get { return bSystemFamily; }
        }

        private StringBuilder vUniqueFamilyInstanceName;
        public StringBuilder GetUniqueFamilyInstanceName 
        {
            get { return vUniqueFamilyInstanceName; }
        }

        //private XYZ vInsertionPoint;
        //public XYZ InsertionPoint 
        //{
        //    get { return vInsertionPoint; }
            
        //}

        //current ELEMENT HOST FILE NAME
        private String vHostName;
        public String HostName
        {
            get { return vHostName; }
            set { vHostName = value; }
        }

        //current Revit Document
        private Document vDoc;
        public Document CurrentRevitDocument
        {
            get { return vDoc; }
            set { vDoc = value; }
        }

        private Transform t = null;
        public Transform FamilyTransform
        {
            get { return t; }
            set { t = value; }
        }


        //private XYZ vBaseX;
        //public XYZ BaseX 
        //{
        //    get{return vBaseX;}
        //    set{vBaseX= new XYZ(value.X,value.Y,value.Z);} 
        //}

        //private XYZ vBaseY;
        //public XYZ BaseY
        //{
        //    get { return vBaseY; }
        //    set { vBaseY = new XYZ(value.X, value.Y, value.Z); }
        //}

        //private XYZ vBaseZ;
        //public XYZ BaseZ
        //{
        //    get { return vBaseZ; }
        //    set { vBaseZ = new XYZ(value.X, value.Y, value.Z); }
        //}


        //private XYZ vOrigin;

        //public XYZ Origin
        //{
        //    get { return vOrigin; }
        //    set { vOrigin = new XYZ(value.X, value.Y, value.Z); }
        //}
        #endregion

        #region class functions

        //private void InstanceSupport()
        //{
                //check type of family is supported for instancing
                //switch (this.objElement.Category.Name.ToString().ToLower())
                //{
                //    case "generic models":
                //        bInstanceSupport=true;
                //        break;
                //    case "doors":
                //        bInstanceSupport = true;
                //        break;
                //    case "mass":
                //        bInstanceSupport = true;
                //        break;
                //    case "windows":
                //        bInstanceSupport = true;
                //        break;
                //    case "planting":
                //        bInstanceSupport = true;
                //        break;
                //    case "furniture":
                //        bInstanceSupport = true;
                //        break;
                //    case "parking":
                //        bInstanceSupport = true;
                //        break;
                //    case "entourage":
                //        bInstanceSupport = true;
                //        break;
                //    case "casework":
                //        bInstanceSupport = true;
                //        break;
                //    case "lighting fixtures":
                //        bInstanceSupport = true;
                //        break;
                //    case "mechanical equipment":
                //        bInstanceSupport = true;
                //        break;
                //    case "plumbing fixtures":
                //        bInstanceSupport = true;
                //        break;
                //    case "specialty equipment":
                //        bInstanceSupport = true;
                //        break;
                //    default:
                //        bInstanceSupport = false;
                //        break;
                //}
        //    bInstanceSupport = true;
        //}

        private void GetTransform(FamilyInstance RevitFamily)
        {
            //get the family transformation
            this.t = RevitFamily.GetTransform();
            //store the inverted base (no idea yet as to why this works)
            //vBaseX = this.t.Inverse.BasisX;
            //vBaseY = this.t.Inverse.BasisY;
            //vBaseZ = this.t.Inverse.BasisZ;

            //get converter class
            //LuxExporter.UnitConverter converter = new UnitConverter();
            //store the translation
            //vOrigin = converter.ConvertPointCoordToMeter(this.t.Origin);
        }

        private void GetData()
        {
            try
            {
                StringBuilder sbValue = new StringBuilder();
                //cast the element to family instance class
                FamilyInstance objRevitFamilyInstance = this.objElement as FamilyInstance;
                // 'add Family name
                vUniqueFamilyInstanceName.Append("FamilyName: "+objRevitFamilyInstance.Symbol.Family.Name.ToLower() + ",");
                //'add type name
                vUniqueFamilyInstanceName.Append("FamilyType: "+objRevitFamilyInstance.Name.ToLower() + ",");
                //'set up list to contain instance parameter
                List<String> lInstanceParameter = new List<String>();
                //loop all parameters in this family and add to instance parameter list
                foreach (Parameter objParameter in objRevitFamilyInstance.Parameters)
                {
                    //'only add parameter which are not of type string
                    //'type string parameter have no impact on visual appearence of family
                    if (objParameter.Definition.ParameterType.ToString() != "Text" && objParameter.Definition.ParameterType.ToString() != "Currency" && objParameter.Definition.ParameterType.ToString() != "URL")
                    {
                        //include level for columns only
                        if (objRevitFamilyInstance.Category.Name.ToString().ToLower() == "columns")
                        {
                            //filter out Phase Created and Phase Demolished
                            if (objParameter.Definition.Name != "Phase Created" && objParameter.Definition.Name != "Phase Demolished")
                            {
                                //'add parameter name and value to list
                                lInstanceParameter.Add("[" + objParameter.Definition.Name.ToString().ToLower() + " : " + ParameterGetValue(objParameter) + "]");

                            }
                        }
                        else
                        {
                            //'filter out Level, Phase Created and Phase Demolished
                            if (objParameter.Definition.Name != "Level" && objParameter.Definition.Name != "Phase Created" && objParameter.Definition.Name != "Phase Demolished")
                            {
                                //'add parameter name and value to list
                                lInstanceParameter.Add("[" + objParameter.Definition.Name.ToString().ToLower() + " : " + ParameterGetValue(objParameter) + "]");

                            }
                        }
                    }
                
                }
                //'sort the instance parameter list
                lInstanceParameter.Sort();

                //'create the unique family instance name
                foreach (String strValue in lInstanceParameter)
                {
                    vUniqueFamilyInstanceName.Append(strValue);
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        string ParameterGetValue (Autodesk.Revit.DB.Parameter _parameter)
        {
            
                //return _value;
                string s;
                switch (_parameter.StorageType)
                {
                    // database value, internal units, e.g. feet:
                    case StorageType.Double: s = RealString(_parameter.AsDouble()); break;
                    case StorageType.Integer: s = _parameter.AsInteger().ToString(); break;
                    case StorageType.String: s = _parameter.AsString(); break;
                    case StorageType.ElementId: s = _parameter.AsElementId().IntegerValue.ToString(); break;
                    case StorageType.None: s = "None"; break;
                    default: Debug.Assert(false, "unexpected storage type"); s = string.Empty; break;
                }
                return s;
            
        }

        public static string RealString(double a)
        {
            return a.ToString("0.##");
        }



        public List<FamilyInstance> GetNestedLightFamilies()
        {
            try
            {
                //'cast the element to family instance class
                FamilyInstance objRevitFamilyInstance = this.objElement as FamilyInstance;
                //get any nested families
                ElementSet NestedFamilies = objRevitFamilyInstance.SubComponents;
                //setup list
                List<FamilyInstance> Lights = new List<FamilyInstance>();

                if (NestedFamilies != null)
                {
                    //loop through nested families and determine type
                    foreach (Element item in NestedFamilies)
                    {
                        //cast as family instanve
                        FamilyInstance objNestedFamilyInstance = item as FamilyInstance;
                        //check type of nested family
                        if (objNestedFamilyInstance.Category.Name.ToString().ToLower() == "lighting fixtures")
                        {
                            Lights.Add(objNestedFamilyInstance);
                            System.Windows.Forms.MessageBox.Show(item.Category.Name.ToString());
                        }

                    }
                }
                if (Lights.Count>0)
                {
                    return Lights;    
                }
                else
                {
                    return null;
                }
                
            }
            catch (Exception)
            {
                return null;
                throw;
            }
            
        }


        public Boolean HasInsertionPoint()
        {
            try
            {
                //'cast the element to family instance class
                FamilyInstance objRevitFamilyInstance = this.objElement as FamilyInstance;
                //'get the xyz insertion point
                LocationPoint objDummy = objRevitFamilyInstance.Location as LocationPoint;
               
                Boolean returnvalue = true;
                if (objDummy == null)
                {
                    //'this is a family with a location curve!!!!
                    //vInsertionPoint = null;
                    returnvalue = false;
                }
                else
                {
                    
                    
                    //convert location point to xyz point
                    //vInsertionPoint = new XYZ(objDummy.Point.X, objDummy.Point.Y, objDummy.Point.Z);
                    
                    //check whether we need to transform the point
                    //if (t != null)
                    //{
                    //    vInsertionPoint = t.OfPoint(vInsertionPoint);
                    //}
                    //convert from feet to m
                    //LuxExporter.UnitConverter uConverter = new UnitConverter();
                    //vInsertionPoint = new XYZ(uConverter.ConvertPointCoordToMeter(vInsertionPoint).X, uConverter.ConvertPointCoordToMeter(vInsertionPoint).Y, uConverter.ConvertPointCoordToMeter(vInsertionPoint).Z);
                    
                    returnvalue = true;
                }

                return returnvalue;
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error while checking for location point");
                return false;
                throw;
            }
        }

        private void RevitSystemFamily()
        {
            //cast the element to family instance class
            try
            {
                FamilyInstance objRevitFamilyInstance = this.objElement as FamilyInstance;
                if (objRevitFamilyInstance!=null)
                {
                    bSystemFamily = false;
                    //bInstanceSupport = true;
                }
                else
                {
                    bSystemFamily = true;
                    //bInstanceSupport = false;
                }
                
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        #endregion

        #region implement
        
        //implement function 'contains' in this class to be used for list search purposes
        public bool Equals(Revit_Instance obj)
        {

            if (this.GetUniqueFamilyInstanceName.ToString() == obj.GetUniqueFamilyInstanceName.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        #endregion

        #region class constructor

        public Revit_Instance(Element objValue, Document tDoc, String LinkName)
        {
            try
            {
                this.objElement = objValue;
                this.vDoc = tDoc;
                this.HostName = LinkName;
                //this.t = null;

                vUniqueFamilyInstanceName = new StringBuilder("");
                //vInsertionPoint = new XYZ();

                //check for System family
                RevitSystemFamily();

                //if non system family get transform and unique name 
                if (!bSystemFamily)
                {
                    //'check for instance support
                    //InstanceSupport();

                    //cast the element to family instance class
                    FamilyInstance objRevitFamilyInstance = this.objElement as FamilyInstance;
                    //store transformation data
                    GetTransform(objRevitFamilyInstance);

                    //if (bInstanceSupport)
                    //{
                        Boolean hasInsertionPoint = HasInsertionPoint();
                        if (!hasInsertionPoint)
                        {
                            //add unique ID as instance name for transform operation
                            vUniqueFamilyInstanceName.Append(objValue.UniqueId.ToString());
                        }
                        else
                        {
                            //get unique name from instance properties
                            GetData();
                        }

                    }
                    else
                    {
                        //add unique ID as instance name for transform operation 
                        vUniqueFamilyInstanceName.Append(objValue.UniqueId.ToString());
                    }

                //get category Name
                vCategoryName = objValue.Category.Name.ToString().ToLower();
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in class constructor:Revit_Instance");
                throw;
            }
            

        }

        public  Revit_Instance(Revit_Instance objValue)
        {
            vUniqueFamilyInstanceName = new StringBuilder("");
            //vInsertionPoint = new XYZ();
            //load values
            //this.vInsertionPoint = objValue.vInsertionPoint;
            this.vUniqueFamilyInstanceName = objValue.GetUniqueFamilyInstanceName;
            this.objElement = objValue.objElement;
            this.bSystemFamily = objValue.ElementIsSystemFamily;

            this.bInstanceSupport = objValue.ElementHasInstanceSupport;
            if (this.bSystemFamily)
            {
                this.bInstanceSupport = false;
            }
            else
            {
                this.bInstanceSupport = true;

            }
            
            //this.BaseX = objValue.BaseX;
            //this.BaseY = objValue.BaseY;
            //this.BaseZ = objValue.BaseZ;
            this.t = objValue.FamilyTransform;
            this.vHostName = objValue.HostName;
        }

        #endregion
    }
}
