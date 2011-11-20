using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace LuxExporter
{
    class NormalOfTriangle
    {
        public XYZ calcNormal (XYZ p1, XYZ p2, XYZ p3)                // Calculates Normal For A Quad Using 3 Points
        {
            int[] numbers = new int[5];
            double[,] v = new double[2,3];
            
            // Calculate The Vector From Point 1 To Point 0
            v[0,0] = p2.X - p1.X;                  // Vector 1.x=Vertex[0].x-Vertex[1].x
            v[0,1] = p2.Y - p1.Y;                  // Vector 1.y=Vertex[0].y-Vertex[1].y
            v[0,2] = p2.Z - p1.Z;                  // Vector 1.z=Vertex[0].y-Vertex[1].z
            // Calculate The Vector From Point 2 To Point 1
            v[1,0] = p3.X - p1.X;                  // Vector 2.x=Vertex[0].x-Vertex[1].x
            v[1,1] = p3.Y - p1.Y;                  // Vector 2.y=Vertex[0].y-Vertex[1].y
            v[1,2] = p3.Z - p1.Z;                   // Vector 2.z=Vertex[0].z-Vertex[1].z

            double[] result = new double[3];
            
            // Compute The Cross Product To Give Us A Surface Normal
            result[0] = v[0, 1] * v[1, 2] - v[0, 2] * v[1, 1];             // Cross Product For Y - Z
            result[1]= v[0, 2] * v[1, 0] - v[0, 0] * v[1, 2];             // Cross Product For X - Z
            result[2] = v[0, 0] * v[1, 1] - v[0, 1] * v[1, 0];             // Cross Product For X - Y

            //check if normal is pointing up rather then down
            if (result[2]<0)
            {
            //flip vector around??
             //Compute The Cross Product To Give Us A Surface Normal
            result[0] = result[0]*-1;             // Cross Product For Y - Z
            result[1] = result[1] * -1;             // Cross Product For X - Z
            result[2] = result[2] * -1;             // Cross Product For X - Y

             }

            //return normalized vector
            result = ReduceToUnit(result);

            //convert to autodesk xyz
            XYZ returnv = new XYZ(result[0],result[1],result[2]);
            return returnv;

        }

        // Reduces A Normal Vector (3 Coordinates)
        // To A Unit Normal Vector With A Length Of One.
        private double[] ReduceToUnit(double[] vector)                  
        {
            // Holds Unit Length                       
            double length;                           
            
            // Calculates The Length Of The Vector
            length = Math.Sqrt((vector[0] * vector[0]) + (vector[1] * vector[1]) + (vector[2] * vector[2]));

            // Prevents Divide By 0 Error By Providing
            if(length == 0.0f)                      
                length = 1.0f;                      // An Acceptable Value For Vectors To Close To 0.
 
            vector[0] /= length;                        // Dividing Each Element By
            vector[1] /= length;                        // The Length Results In A
            vector[2] /= length;                        // Unit Normal Vector.

            return vector;
        
        }
    }
}
