using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuxExporter
{
    class LuxExporter_ScreenWindow
    {
            private Double vscreenaspectratio;
            private Double vlensshiftx; 
            private Double vlensshifty;
            private Double vscale;

            public Double[] GetScreenWindow()
            {
                Double[] dbScreenWindow = new Double[4];
                Double dbInvertScreenRatio=1/vscreenaspectratio;
                if (vscreenaspectratio <1)
	            {
                    dbScreenWindow[0] = (2 * vlensshiftx - vscreenaspectratio) * vscale;
                    dbScreenWindow[1] = (2 * vlensshiftx + vscreenaspectratio) * vscale;
                    dbScreenWindow[2] = (2 * vlensshifty - 1) * vscale;
                    dbScreenWindow[3] = (2 * vlensshifty + 1) * vscale;
	            }
                else
                {
                    dbScreenWindow[0] = (2 * vlensshiftx - 1) * vscale;
                    dbScreenWindow[1] = (2 * vlensshiftx + 1) * vscale;
                    dbScreenWindow[2] = (2 * vlensshifty - dbInvertScreenRatio) * vscale;
                    dbScreenWindow[3] = (2 * vlensshifty + dbInvertScreenRatio) * vscale;
                }
                return dbScreenWindow;
            }
        public LuxExporter_ScreenWindow(Double screenaspectratio , Double lensshiftx, 
             Double lensshifty, Double scale )
        {
            vscreenaspectratio = screenaspectratio;
            vlensshiftx = lensshiftx;
            vlensshifty = lensshifty;
            //revit draws in mm...convert scale
            vscale = scale/1000.00;
        }
        
    }
}
