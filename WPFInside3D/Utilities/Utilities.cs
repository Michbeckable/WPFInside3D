using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace WPFInside3D
{
    public class Utilities
    {
        public static RayMeshGeometry3DHitTestResult PickMouse(Visual reference, Point p)
        {
            HitTestResult hitRes = VisualTreeHelper.HitTest(reference, p);
            if (hitRes != null)
            {
                if (hitRes is RayMeshGeometry3DHitTestResult)
                {
                    RayMeshGeometry3DHitTestResult meshHitRes = (RayMeshGeometry3DHitTestResult)hitRes;
                    return meshHitRes;
                }
            }

            return null;
        }
    }
}
