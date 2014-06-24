using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPFInside3D
{
    namespace Geometry
    {
        public class Triangle
        {
            public Triangle(Point3D point1, Point3D point2, Point3D point3)
            {
                // structs (like Point3D) are value types, therefore we can use the given points here without cloning them
                // that might be the reason why structs have no cloning method
                this.p1 = point1;
                this.p2 = point2;
                this.p3 = point3;
                this.BuildMesh();
            }

            private void BuildMesh()
            {
                this.mesh = new MeshGeometry3D();

                this.mesh.Positions.Add(this.p1);
                this.mesh.Positions.Add(this.p2);
                this.mesh.Positions.Add(this.p3);
                // add the normal for every point
                this.normal = CalcNormal(this.p1, this.p2, this.p3);
                this.mesh.Normals.Add(this.normal);
                this.mesh.Normals.Add(this.normal);
                this.mesh.Normals.Add(this.normal);

                this.mesh.TriangleIndices.Add(0);
                this.mesh.TriangleIndices.Add(1);
                this.mesh.TriangleIndices.Add(2);
            }

            public static Vector3D CalcNormal(Point3D p1, Point3D p2, Point3D p3)
            {
                // we assume the points are in ccw order
                // construct two vectors from them (p1-->p2) and (p2-->p3) and make cross product 
                // to get normal pointing in the right direction (in right hand coords system)
                Vector3D n = Vector3D.CrossProduct(p2 - p1, p3 - p2);
                n.Normalize();
                return n;
            }

            public Point3D p1 { get; private set; }
            public Point3D p2 { get; private set; }
            public Point3D p3 { get; private set; }
            public Vector3D normal { get; private set; }

            public MeshGeometry3D mesh { get; private set; }
        }

        public abstract class TriangleModel : List<Triangle>
        {
            protected void BuildModel()
            {
                this.model = new Model3DGroup();
                foreach (Triangle t in this)
                    model.Children.Add(new GeometryModel3D(t.mesh, (this.material == null) ? defaultMaterial : this.material));

                Transform3DGroup tGroup = new Transform3DGroup();
                this.model.Transform = tGroup;
            }

            protected void CenterModel(Vector3D vToCenter)
            {
                Transform3DGroup tGroup = (Transform3DGroup)this.model.Transform;
                if (tGroup != null)
                    tGroup.Children.Add(new TranslateTransform3D(vToCenter));
            }

            public Material material { get; protected set; }
            public Model3DGroup model { get; protected set; }

            protected static Material defaultMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Red));  // is this correct???
        }

        public class Plane : TriangleModel
        {
            public Plane()
                : this(10.0, 10.0)
            {
            }

            public Plane(double l, double w)
            {
                this.length = l;
                this.width = w;

                this.BuildMesh();
            }

            private void BuildMesh()
            {
                Point3D p1 = new Point3D(0.0, 0.0, 0.0);
                Point3D p2 = new Point3D(length, 0.0, 0.0);
                Point3D p3 = new Point3D(length, 0.0, -width);
                Point3D p4 = new Point3D(0.0, 0.0, -width);

                this.Add(new Triangle(p1, p3, p4));
                this.Add(new Triangle(p1, p2, p3));
                this.BuildModel();

                this.CenterModel(new Vector3D(length * -0.5, 0.0, width * 0.5));
            }

            public double length { get; private set; }
            public double width { get; private set; }
        }

        public class Cube : TriangleModel
        {
            public Cube()
                : this(10.0, 10.0, 10.0)
            { }

            public Cube(double l, double w, double h)
            {
                this.length = l;
                this.width = w;
                this.height = h;
                this.BuildMesh();
            }

            public void BuildMesh()
            {
                Point3D p1 = new Point3D(0.0, 0.0, 0.0);
                Point3D p2 = new Point3D(length, 0.0, 0.0);
                Point3D p3 = new Point3D(length, 0.0, -width);
                Point3D p4 = new Point3D(0.0, 0.0, -width);

                Point3D p5 = new Point3D(0.0, height, 0.0);
                Point3D p6 = new Point3D(length, height, 0.0);
                Point3D p7 = new Point3D(length, height, -width);
                Point3D p8 = new Point3D(0.0, height, -width);

                // bottom
                this.Add(new Triangle(p1, p4, p3));
                this.Add(new Triangle(p1, p3, p2));
                // top
                this.Add(new Triangle(p5, p7, p8));
                this.Add(new Triangle(p5, p6, p7));
                // front
                this.Add(new Triangle(p1, p2, p6));
                this.Add(new Triangle(p1, p6, p5));
                // back
                this.Add(new Triangle(p4, p8, p7));
                this.Add(new Triangle(p4, p7, p3));
                // left
                this.Add(new Triangle(p1, p5, p8));
                this.Add(new Triangle(p1, p8, p4));
                // right
                this.Add(new Triangle(p2, p7, p6));
                this.Add(new Triangle(p2, p3, p7));

                this.BuildModel();
                this.CenterModel(new Vector3D(length * -0.5, height * -0.5, width * 0.5));
            }

            public double length { get; private set; }
            public double width { get; private set; }
            public double height { get; private set; }

        }
    }
}