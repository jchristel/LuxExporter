using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace LuxExporter
{
    class UnitConverter
    {
        
        public XYZ ConvertPointCoordToMeter(XYZ pPoint)
        {
            XYZ pDummy = new XYZ(pPoint.X * 0.3048, pPoint.Y * 0.3048, pPoint.Z * 0.3048);
            return pDummy;
            //return pPoint;
        }

        public UV ConvertUVCoordToMeter(UV pPoint)
        {
            UV pDummy = new UV(pPoint.U * 0.3048, pPoint.V * 0.3048);
            return pDummy;
        }

        public Double ConvertRadiantoDegree(Double lValue)
        {
            lValue = lValue * 180 / Math.PI;
            return lValue;
        }

        public XYZ InvertPoint(XYZ pPoint)
        {
            XYZ pDummy = new XYZ(pPoint.X * -1, pPoint.Y * -1, pPoint.Z * -1);
            return pDummy;
        }
        
    }
}
