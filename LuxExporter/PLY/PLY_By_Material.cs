using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace LuxExporter.PLY
{
    class PLY_By_Material
    {
        #region class properties
        
        //property containing the material ID for mesh
        private Autodesk.Revit.DB.ElementId vMaterialID;
        public Autodesk.Revit.DB.ElementId MaterialID 
        {
            get { return vMaterialID; }
            set {vMaterialID=value;}
        }

        //flag indicating whether this is a Glass2 object
        private Boolean vIs_Glass2 = false;
        public Boolean Is_Glass2 
        {
            get { return vIs_Glass2; }
            set { vIs_Glass2 = value; }
        }

        private String sMaterialName;
        //Property containing the material name
        public String MaterialName 
        {
            get { return sMaterialName; }
            set { sMaterialName = value; }
        }


        private Int32 iNumberOfFaces = 0;
        public Int32 NoofFaces 
        { 
            get {return iNumberOfFaces;}
        }

        //setup class storing ply data
        private LuxExporter.PLY.PLY_Data Data;

       
        #endregion

        #region class functions
        //function processing the meash


        public void TriangulateTopoMesh(Autodesk.Revit.DB.Mesh vMesh)
        {


            //setup utility class
            LuxExporter.UnitConverter Converter = new UnitConverter();
            
            // set pointer
            Data.NumberOfVerticesinFace = 3;
            
            //export all triangles
            for (int i = 0; i < vMesh.NumTriangles; i++)
            {
                //increase face counter
                iNumberOfFaces++;

                //create a triangle
                MeshTriangle objTriangular = vMesh.get_Triangle(i);

                //calculate normal of triangle
                LuxExporter.NormalOfTriangle NormalofT = new NormalOfTriangle();
                XYZ NormalAtPoint =  NormalofT.calcNormal(objTriangular.get_Vertex(0), objTriangular.get_Vertex(1), objTriangular.get_Vertex(2));
                   
                for (int iPointsCounter = 0; iPointsCounter < 3; iPointsCounter++)
                {
                    XYZ point = objTriangular.get_Vertex(iPointsCounter);
                    //convert to meter
                    point = Converter.ConvertPointCoordToMeter(point);
                    //add to ply class
                    Data.AddVertice(point, NormalAtPoint);
                    
                    //add to ply class
                    //Data.AddVertice(point);

               } 
            }
        }

        public void TriangulateFace(Autodesk.Revit.DB.Face vFace, Transform instTransform)
        {
            try
            {
                //setup utility class
                LuxExporter.UnitConverter Converter = new UnitConverter();

                //process face
                Mesh vMesh = vFace.Triangulate();

                


                //check if we have a quad mesh (4 edges to a face)
                if (vMesh.Vertices.Count == 4)
                {
                    //increase face counter
                    iNumberOfFaces++;

                    //found quad
                    Data.NumberOfVerticesinFace = 4;
                    //loop through all vertices and add
                    foreach (XYZ ii in vMesh.Vertices)
                    {
                        XYZ point = ii;
                        XYZ transformedPoint;


                        //transformedPoint = point;


                        if (instTransform == null)
                        {
                            transformedPoint = point;
                        }
                        else
                        {
                            transformedPoint = instTransform.OfPoint(point);
                        }

                        //get the normal
                        XYZ NormalAtPoint;

                        IntersectionResult IntResult= vFace.Project(ii);

                        if (IntResult != null)
                        {
                            UV UVatPoint = IntResult.UVPoint;
                            NormalAtPoint = new XYZ(vFace.ComputeNormal(UVatPoint).X, vFace.ComputeNormal(UVatPoint).Y, vFace.ComputeNormal(UVatPoint).Z);
                        }
                        else
                        {
                            //this needs fixing!!!
                            NormalAtPoint = new XYZ(0, 0, 0);
                        } 
                       

                        //convert to meter
                        transformedPoint = Converter.ConvertPointCoordToMeter(transformedPoint);
                        //NormalAtPoint = Converter.ConvertPointCoordToMeter(NormalAtPoint);
                        
                        //add to ply class
                        Data.AddVertice(transformedPoint,NormalAtPoint);
                        //add to ply class
                        //Data.AddVertice(transformedPoint);
                    }

                }
                else
                {
                    // set pointer
                    Data.NumberOfVerticesinFace = 3;


                    //export all triangles in face
                    for (int i = 0; i < vMesh.NumTriangles; i++)
                    {
                        //increase face counter
                        iNumberOfFaces++;

                        MeshTriangle objTriangular = vMesh.get_Triangle(i);

                        for (int iPointsCounter = 0; iPointsCounter < 3; iPointsCounter++)
                        {
                            XYZ point = objTriangular.get_Vertex(iPointsCounter);

                            XYZ transformedPoint;

                            if (instTransform == null)
                            {
                                transformedPoint = point;
                            }
                            else
                            {
                                transformedPoint = instTransform.OfPoint(point);
                            }

                            XYZ NormalAtPoint;

                            //get the normal
                            IntersectionResult IntResult = vFace.Project(point);
                            if (IntResult!=null)
                            {
                                UV UVatPoint = IntResult.UVPoint;
                                NormalAtPoint = new XYZ(vFace.ComputeNormal(UVatPoint).X,vFace.ComputeNormal(UVatPoint).Y,vFace.ComputeNormal(UVatPoint).Z);
                            }
                            else
                            {
                                //this needs fixing
                                NormalAtPoint = new XYZ(0, 0, 0);
                            }

                            //convert to meter
                            transformedPoint = Converter.ConvertPointCoordToMeter(transformedPoint);
                            //NormalAtPoint = Converter.ConvertPointCoordToMeter(NormalAtPoint);
                            //add to ply class
                            Data.AddVertice(transformedPoint,NormalAtPoint);
                            //add to ply class
                            //Data.AddVertice(transformedPoint);


                        }

                    }
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error in module PLY_by_material::TriangulateMesh "+ex.Message);
                throw;
            }
           
        

        }

        public List<String> GetVerticesList()
        {
            return Data.GetVerticesList;
        }

        public List<String> GetIndecisList()
        {
            return Data.GetVerticesIndexList;
        }

        public Int32 NoOfVertice()
        {
            return Data.NoofVertices;
        }

        #endregion
        #region class constructors


        //class constructor (takes Revit Element as argument)
        public PLY_By_Material()
        {
            Data = new PLY_Data();
        }
        #endregion
    }
}
