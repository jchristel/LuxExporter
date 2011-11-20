using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace LuxExporter
{
    class Revit_BoundingBox_Checker
    {
        public Boolean BoundingBox_Checker(BoundingBoxXYZ item, BoundingBoxXYZ View)
        {
            try
            {
                //check whether item MIN x,y,z values are bigger then view X,y,z values
                Boolean MIN = false;
                if (item.Min.X>View.Min.X && item.Min.Y>View.Min.Y &&item.Min.Z>View.Min.Z)
                {
                    MIN = true;
                }
                //check whether item max x,y,z values are smaller then view x,y,z values
                Boolean Max = false;
                if (item.Max.X<View.Max.X&&item.Max.Y<View.Max.Y&&item.Max.Z<View.Max.Z)
                {
                    Max = true;
                }

                if (MIN==true && Max ==true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
               
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
    }
}
