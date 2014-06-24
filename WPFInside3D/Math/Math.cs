using System;
using System.Windows.Media.Media3D;

namespace WPFInside3D
{
    namespace Math
    {
        public class SphereCoords
        {
            #region properties

            public double Radius
            {
                get { return this.radius; }
                set
                {
                    this.radius = value;
                    this.RefreshCartesian();
                }
            }

            /// <summary>
            /// azimuth angle
            /// </summary>
            public double Phi
            {
                get { return this.phi; }
                set
                {
                    this.phi = value;
                    this.RefreshCartesian();
                }
            }

            /// <summary>
            /// polar angle 
            /// </summary>
            public double Theta
            {
                get { return this.theta; }
                set
                {
                    if (value < System.Math.PI && value > 0.0)
                    {
                        this.theta = value;
                        this.RefreshCartesian();
                    }
                }
            }

            public Vector3D VectorCartesian { get; private set; }

            #endregion

            #region ctor

            public SphereCoords()
                : this(1.0, 0.0, 0.0)
            { }

            public SphereCoords(double r, double p, double t)
            {
                this.radius = r;
                this.phi = p;
                this.theta = t;
                this.RefreshCartesian();
            }

            #endregion

            #region methods

            public void RefreshCartesian()
            {
                this.VectorCartesian = ToCartesianCoords(this);
            }

            #endregion

            #region conversion from/to cartesian coordinates

            public static SphereCoords FromCartesianCoords(Vector3D vCartesian, bool yIsUp = true)
            {
                SphereCoords coords = new SphereCoords();
                // in 3d right hand coord system y is usually the up axis, but in sphere coords theory z is up
                // so map the different coordinate systems
                //Vector3D p = new Point3D(pCartesian.X, pCartesian.Y, pCartesian.Z);  
                if (yIsUp)
                {
                    double y = vCartesian.Y;
                    vCartesian.Y = -vCartesian.Z;
                    vCartesian.Z = y;
                }

                coords.Radius = vCartesian.Length;
                coords.Phi = System.Math.Atan2(vCartesian.Y, vCartesian.X);
                if (coords.Phi < 0)
                    coords.Phi += 2 * System.Math.PI;
                coords.Theta = System.Math.Acos(vCartesian.Z / coords.Radius);

                return coords;
            }

            public static Vector3D ToCartesianCoords(SphereCoords coords, bool yIsUp = true)
            {
                Vector3D v = new Vector3D();
                v.X = coords.radius * System.Math.Sin(coords.theta) * System.Math.Cos(coords.phi);
                v.Y = coords.radius * System.Math.Sin(coords.theta) * System.Math.Sin(coords.phi);
                v.Z = coords.radius * System.Math.Cos(coords.theta);

                // re-map the coords systems
                if (yIsUp)
                {
                    double z = v.Z;
                    v.Z = -v.Y;
                    v.Y = z;
                }

                return v;
            }

            #endregion

            #region private fields

            private double radius;
            private double phi;
            private double theta;

            #endregion

        }
    }
}
