using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace LuxExporter.PLY
{
    class PLY_Data
    {
        #region properties
        
        //list containing VerticesCorrdinates
        private List<String> oVerticesList;
        //list containing vertices index
        private List<String> oVerticesIndexList;
        //pointer for current vertice number
        private int iVerticeCounter = 0;

        // string containing vertices index 
        private StringBuilder sVerticeIndex = new StringBuilder("");

        //number of vertices in group (triangle or quad)
        private Int16 iNumberOfVerticesinFace=3;
        public Int16 NumberOfVerticesinFace
        {
            get { return iNumberOfVerticesinFace; }
            set { iNumberOfVerticesinFace = value; }
        }

        //property returning no of vertices in mesh
        public Int32 NoofVertices 
        {
            get { return oVerticesList.Count; }
            
        }

        //property returning Vertices List
        public List<String> GetVerticesList 
        {
            get 
            {
                if (oVerticesList.Count>0)
                {
                    return oVerticesList;   
                }
                else
                {
                    return null;
                }
                }
            
        }

        //property returning Vertices Index List
        public List<String> GetVerticesIndexList
        {
            get
            {
                if (oVerticesIndexList.Count > 0)
                {
                    return oVerticesIndexList;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion


        //function adding vertice to vertices List
        public void AddVertice(XYZ transformedPoint)
        {
            try
            {
                //convert to string            
                String strPointCoordinates = transformedPoint.X.ToString("f6") + " " + transformedPoint.Y.ToString("f6") + " " + transformedPoint.Z.ToString("f6");
                AddVerticeWorker(strPointCoordinates);
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in Module PLY_Data::AddVertice");
                throw;
            }
        }
        
        //function adding vertice to vertices List
        public void AddVertice(XYZ transformedPoint, XYZ NormalAtPoint)
        {
            try
            {
                //convert to string            
                String strPointCoordinates = transformedPoint.X.ToString("f6") + " " + transformedPoint.Y.ToString("f6") + " " + transformedPoint.Z.ToString("f6")
                                            + " " + NormalAtPoint.X.ToString("f6") + " " + NormalAtPoint.Y.ToString("f6") + " " + NormalAtPoint.Z.ToString("f6");
                AddVerticeWorker(strPointCoordinates);              
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in Module PLY_Data::AddVertice");
                throw;
            }
            
            
        }

        private void AddVerticeWorker(String strPointCoordinates)
        {
            try
            {
            Int32 iIndex = 0;

                //add vertice coordinates to List if required
                if (oVerticesList.Count > 0)
                {
                    //check whether point exists
                    if (!oVerticesList.Contains(strPointCoordinates))
                    {
                        oVerticesList.Add(strPointCoordinates);
                    }
                    //set index value for point
                    iIndex = oVerticesList.IndexOf(strPointCoordinates);
                }
                else
                {
                    //ADD FIRST POINT
                    oVerticesList.Add(strPointCoordinates);
                    iIndex = 0;
                }
                int i = 1;

                iVerticeCounter = iVerticeCounter + i;
                if (iVerticeCounter == iNumberOfVerticesinFace)
                {
                    //complete string 
                    sVerticeIndex.Append(iIndex.ToString());
                    // index string for triangle complete
                    oVerticesIndexList.Add(sVerticeIndex.ToString());
                    iVerticeCounter = 0;
                    sVerticeIndex.Clear();
                }
                else
                {
                    //add number of vertices first
                    if (iVerticeCounter == 1)
                    {
                        sVerticeIndex.Append(iNumberOfVerticesinFace.ToString() + " ");
                    }
                    // add index value
                    sVerticeIndex.Append(iIndex.ToString() + " ");
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in Module PLY_Data::AddVertice");
                throw;
            }
        }

        
        #region class constructors
        
        //class constructor (takes Revit Element as argument)
        public PLY_Data()
        {
            oVerticesList = new List<String>();
            oVerticesIndexList = new List<String>();
        }
        #endregion
    }
}
