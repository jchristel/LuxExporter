using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace LuxExporter
{
    class Revit_Transform
    {
             
        //this function takes the element transform and convertes it into 
        // a lux transformation
        // for some reason it requires the inverse transformation to work...

        public String TransformationInHost(Transform ElementTransform)
        {
            try
            {
                //convert origin to meter
                LuxExporter.UnitConverter Converter = new UnitConverter();
                XYZ ConvertedOrigin = Converter.ConvertPointCoordToMeter(ElementTransform.Origin);
                
                //invert Revit Transform
                XYZ BaseX = ElementTransform.Inverse.BasisX;
                XYZ BaseY = ElementTransform.Inverse.BasisY;
                XYZ BaseZ = ElementTransform.Inverse.BasisZ;

                //return the lux transformation values as 4 x 4 matrix derived from base and origin of the transformation
                return BaseX.X.ToString("f6") + " " + BaseY.X.ToString("f6") + " " + BaseZ.X.ToString("f6") + " 0.0" + " " +
                    BaseX.Y.ToString("f6") + " " + BaseY.Y.ToString("f6") + " " + BaseZ.Y.ToString("f6") + " 0.0" + " " +
                    BaseX.Z.ToString("f6") + " " + BaseY.Z.ToString("f6") + " " + BaseZ.Z.ToString("f6") + " 0.0" + " " +
                    ConvertedOrigin.X.ToString("f6") + " " + ConvertedOrigin.Y.ToString("f6") + " " + ConvertedOrigin.Z.ToString("f6") + " 1.0";
            }
            catch (Exception)
            {
                return null;
                throw;
            }

        }


        //this function takes the element transform and a revit link transform and convertes it into 
        // a lux transformation
        // for some reason it requires the inverse transformation to work...

        public String TransformationInLink(LuxExporter.Revit_Instance objInstance, Transform LinkTransform)
        {
            try
            {
                Transform CrossT = objInstance.FamilyTransform.Multiply(LinkTransform.Inverse);
                //get the base point for the translation vector
                LuxExporter.UnitConverter Converter = new UnitConverter();
                XYZ ConvertedOriginLink = Converter.ConvertPointCoordToMeter(LinkTransform.Origin);
                //get the element transform
                Transform ElementTransform = objInstance.FamilyTransform;
                //convert element origin to meter
                XYZ ConvertedOriginElement = Converter.ConvertPointCoordToMeter(ElementTransform.Origin);

                //cross product of link and element transform
                Double Tx = LinkTransform.Inverse.BasisX.X * ConvertedOriginElement.X + LinkTransform.Inverse.BasisX.Y * ConvertedOriginElement.Y + LinkTransform.Inverse.BasisX.Z * ConvertedOriginElement.Z + ConvertedOriginLink.X;
                Double Ty = LinkTransform.Inverse.BasisY.X * ConvertedOriginElement.X + LinkTransform.Inverse.BasisY.Y * ConvertedOriginElement.Y + LinkTransform.Inverse.BasisY.Z * ConvertedOriginElement.Z + ConvertedOriginLink.Y;
                Double Tz = LinkTransform.Inverse.BasisZ.X * ConvertedOriginElement.X + LinkTransform.Inverse.BasisZ.Y * ConvertedOriginElement.Y + LinkTransform.Inverse.BasisZ.Z * ConvertedOriginElement.Z + ConvertedOriginLink.Z;
                
                //build transformation String
                return CrossT.BasisX.X.ToString("f6") + " " + CrossT.BasisY.X.ToString("f6") + " " + CrossT.BasisZ.X.ToString("f6") + " 0.0" + " " +
                    CrossT.BasisX.Y.ToString("f6") + " " + CrossT.BasisY.Y.ToString("f6") + " " + CrossT.BasisZ.Y.ToString("f6") + " 0.0" + " " +
                    CrossT.BasisX.Z.ToString("f6") + " " + CrossT.BasisY.Z.ToString("f6") + " " + CrossT.BasisZ.Z.ToString("f6") + " 0.0" + " " +
                    Tx.ToString("f6") + " " + Ty.ToString("f6") + " " + Tz.ToString("f6") + " 1.0";



            }
            catch (Exception)
            {
                return null;
                throw;
            }

        }

        //public String CalculateLuxTransformationMatrixInLinkOther(LuxExporter.Revit_Instance objInstance, Transform LinkTransform)
        //{
        //    try
        //    {
        //        //Transform CrossProduct = objInstance.FamilyTransform.Inverse.Multiply(LinkTransform.Inverse);
        //        XYZ Bx = objInstance.BaseX;
        //        XYZ By = objInstance.BaseY;
        //        XYZ Bz = objInstance.BaseZ;
        //        XYZ Bo = objInstance.Origin;


        //        XYZ Lx = LinkTransform.Inverse.BasisX;
        //        XYZ Ly = LinkTransform.Inverse.BasisY;
        //        XYZ Lz = LinkTransform.Inverse.BasisZ;

        //        LuxExporter.UnitConverter Converter = new UnitConverter();
        //        XYZ Lo = Converter.ConvertPointCoordToMeter(LinkTransform.Origin);

        //        //first column 
        //        Double CrossXX = Bx.X * Lx.X + Bx.Y * Ly.X + Bx.Z * Lz.X + Bo.X * 0;
        //        Double CrossXY = By.X * Lx.X + By.Y * Ly.X + By.Z * Lz.X + Bo.Y * 0;
        //        Double CrossXZ = Bz.X * Lx.X + Bz.Y * Ly.X + Bz.Z * Lz.X + Bo.Z * 0;
        //        //Double CrossXN = 0.0 * Lx.X + 0.0* Ly.X + 0.0 * Lz.X + 1 * 0.0;

        //        //second column
        //        Double CrossYX = Bx.X * Lx.Y + Bx.Y * Ly.Y + Bx.Z * Lz.Y + Bo.X * 0;
        //        Double CrossYY = By.X * Lx.Y + By.Y * Ly.Y + By.Z * Lz.Y + Bo.Y * 0;
        //        Double CrossYZ = Bz.X * Lx.Y + Bz.Y * Ly.Y + Bz.Z * Lz.Y + Bo.Z * 0;
        //        //Double CrossYN = 0.0 * Lx.Y + 0.0 * Ly.Y + 0.0 * Lz.Y + 1 * 0.0;

        //        //third column
        //        Double CrossZX = Bx.X * Lx.Z + Bx.Y * Ly.Z + Bx.Z * Lz.Z + Bo.X * 0;
        //        Double CrossZY = By.X * Lx.Z + By.Y * Ly.Z + By.Z * Lz.Z + Bo.Y * 0;
        //        Double CrossZZ = Bz.X * Lx.Z + Bz.Y * Ly.Z + Bz.Z * Lz.Z + Bo.Z * 0;
        //        //Double CrossZN = 0.0 * Lx.Z + 0.0 * Ly.Z + 0.0 * Lz.Z + 1 * 0.0;

        //        Double RCrossNX = Bx.X * Lo.X + Bx.Y * Lo.Y + Bx.Z * Lo.Z + Bo.X * 1;
        //        Double RCrossNY = By.X * Lo.X + By.Y * Lo.Y + By.Z * Lo.Z + Bo.Y * 1;
        //        Double RCrossNZ = Bz.X * Lo.X + Bz.Y * Lo.Y + Bz.Z * Lo.Z + Bo.Z * 1;
        //        //Double RCrossNN = Bx.X * Lo.X + Bx.Y * Lo.Y + Bx.Z * Lo.Z + Bo.X * 1

        //        return CrossXX.ToString("f6") + " " + CrossYX.ToString("f6") + " " + CrossZX.ToString("f6") + " 0.0" + " " +
        //            CrossXY.ToString("f6") + " " + CrossYY.ToString("f6") + " " + CrossZY.ToString("f6") + " 0.0" + " " +
        //            CrossXZ.ToString("f6") + " " + CrossYZ.ToString("f6") + " " + CrossZZ.ToString("f6") + " 0.0" + " " +
        //            RCrossNX.ToString("f6") + " " + RCrossNY.ToString("f6") + " " + RCrossNZ.ToString("f6") + " 1.0";



        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //        throw;
        //    }

        //}

        public Revit_Transform()
        {
            
            
        }
    }
}
