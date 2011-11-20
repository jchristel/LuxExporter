using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace LuxExporter
{
    class Revit_View_ThreeD
    {
        //container for view object
        private View3D vView3D;
        //converter from feet to mm
        private LuxExporter.UnitConverter Converter = new UnitConverter();
        
        #region Class Properties
        
        private XYZ vCameraPosition;
        public XYZ CameraPosition 
        {
            get { return vCameraPosition; }
            set 
            { 
                vCameraPosition = value;
                vCameraPosition = Converter.ConvertPointCoordToMeter(vCameraPosition);
            }
        }


        private XYZ vCameraViewDirection;
        public XYZ CameraViewDirection 
        {
            get { return vCameraViewDirection; }
            set 
            { 
                vCameraViewDirection = value;
                //negate vector
                XYZ oDummy = new XYZ(vCameraViewDirection.X* -1 +vView3D.EyePosition.X,
                    vCameraViewDirection.Y* -1 +vView3D.EyePosition.Y,
                    vCameraViewDirection.Z* -1 +vView3D.EyePosition.Z);
                //comvert to meter
                oDummy = Converter.ConvertPointCoordToMeter(oDummy);
                vCameraViewDirection = oDummy;
                
            }
            
        }


        private XYZ vCameraUp;
        public XYZ CameraUp 
        {
            get { return vCameraUp; }
            set 
            { 
                vCameraUp = value;
                //convert to meter
                vCameraUp = Converter.ConvertPointCoordToMeter(vCameraUp);
            }
        }

        #endregion

        # region class constructor
        //class constructor
        public Revit_View_ThreeD (View3D view)
        {
            vView3D = view;
            vCameraPosition=vView3D.EyePosition;
            vCameraViewDirection = vView3D.ViewDirection;
            vCameraUp = vView3D.UpDirection;
        }
        #endregion

    }
}
