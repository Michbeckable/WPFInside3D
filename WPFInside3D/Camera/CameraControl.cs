using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFInside3D
{
    using WPFInside3D.Math;

    public class CameraControl
    {
        #region properties

        public Transform3D Transform { get; private set; }

        public Point3D Position { get; private set; }
        public Point3D Target { get; private set; }
        public SphereCoords coords { get; private set; }

        public enum Mode { idle, pan, rotate, zoom }
        public Mode mode { get; private set; }

        public bool ZoomToObject { get; set; }
        public double multPan { get; set; }
        public double multRotate { get; set; }
        public double multZoom { get; set; }

        private Point mousePosOld { get; set; }

        #endregion

        #region ctor

        public CameraControl(Point3D pos, Point3D lookAt)
        {
            this.Target = lookAt;
            this.Position = pos;

            // init sphere coordinates
            this.coords = SphereCoords.FromCartesianCoords(-(this.Target - this.Position));

            // init transformations
            Transform3DGroup allTransforms = new Transform3DGroup();
            translateToPos = new TranslateTransform3D();
            this.rotateLookAt = new MatrixTransform3D();
            this.matLookAt = new Matrix3D();
            allTransforms.Children.Add(this.rotateLookAt);   // first Rotation 
            allTransforms.Children.Add(translateToPos);      // second translation 
            this.Transform = allTransforms;
            this.UpdateTransformations();

            this.ZoomToObject = true;
            this.multPan = 0.05;
            this.multRotate = 0.01;
            this.multZoom = 1.0;

            this.mode = Mode.idle;
        }

        #endregion

        #region mouse event handling

        public void SetMode(Mode theMode, Point pMouse)
        {
            this.mode = theMode;
            this.mousePosOld = pMouse;
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            switch (this.mode)
            {
                case Mode.idle:
                    break;
                case Mode.pan:
                    this.Pan(e.GetPosition((UIElement)e.Source));
                    break;
                case Mode.rotate:
                    this.Rotate(e.GetPosition((UIElement)e.Source));
                    break;
            }
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.SetMode(CameraControl.Mode.rotate, e.GetPosition((UIElement)e.Source));
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                this.SetMode(CameraControl.Mode.pan, e.GetPosition((UIElement)e.Source));
            }
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            this.SetMode(CameraControl.Mode.idle, e.GetPosition((UIElement)e.Source));
        }

        public void OnMouseWheel(MouseWheelEventArgs e)
        {
            this.Zoom(e.GetPosition((UIElement)e.Source), e.Delta, (UIElement)e.Source);
        }

        #endregion

        #region transformations

        public void Rotate(Point pMouse)
        {
            Vector vDelta = (this.mousePosOld - pMouse);
            this.mousePosOld = pMouse;

            this.coords.Phi += this.multRotate * vDelta.X;
            this.coords.Theta += this.multRotate * -vDelta.Y;
            this.UpdateTransformations();
        }

        public void Pan(Point pMouse)
        {
            Vector vDelta = (this.mousePosOld - pMouse);
            Vector3D vPan = new Vector3D(vDelta.X * this.multPan, vDelta.Y * this.multPan, 0.0);
            vPan.Y *= -1;
            this.mousePosOld = pMouse;

            // vMove describes movement in camera coords (x,y)
            // we have to apply changes to camera position/target in world coords
            vPan = this.matLookAt.Transform(vPan);
            this.Target += vPan;
            this.Position += vPan;
            this.UpdateTransformations(false);
        }

        public void Zoom(Point pMouse, double delta, Visual reference)
        {
            // zoom to object if hit with mouse pointer
            RayMeshGeometry3DHitTestResult pickRes = Utilities.PickMouse(reference, pMouse);
            double oldRadius = this.coords.Radius;
            double diffZoom = delta;

            // limit zoom 
            if (diffZoom < 0 && this.coords.Radius >= 100)
                return;

            // change radius to zoom
            double radiusMultiplier = System.Math.Abs(diffZoom) * -0.000416 + 1;
            this.coords.Radius = (diffZoom > 0) ? this.coords.Radius * radiusMultiplier : this.coords.Radius / radiusMultiplier;

            if (pickRes != null && this.ZoomToObject)
            {
                Vector3D vPickResPoint = new Vector3D(pickRes.PointHit.X, pickRes.PointHit.Y, pickRes.PointHit.Z);
                Vector3D vTarget = new Vector3D(this.Target.X, this.Target.Y, this.Target.Z);
                Vector3D t = Vector3D.Subtract(vPickResPoint, vTarget);
                double tAddLength = t.Length - t.Length / oldRadius * this.coords.Radius;
                t.Normalize();
                t = t * tAddLength;
                this.Target += t;
            }

            this.UpdateTransformations();
        }

        public void UpdateTransformations(bool sphereCoordsChanged = true)
        {
            if (sphereCoordsChanged)
            {
                Vector3D v = this.coords.VectorCartesian;
                this.Position = this.Target + v;
                // target and/or position have changed: update orientation matrix
                this.UpdateLookAtMatrix();
            }

            // update translation to position in matrix
            this.translateToPos.OffsetX = this.Position.X;
            this.translateToPos.OffsetY = this.Position.Y;
            this.translateToPos.OffsetZ = this.Position.Z;
        }

        private void UpdateLookAtMatrix()
        {
            // construct lookat matrix
            Vector3D up = new Vector3D(0.0, 1.0, 0.0);
            Vector3D zCamCoords = -(this.Target - this.Position);   // the viewing direction along negative z-axis in camera coords!
            zCamCoords.Normalize();

            Vector3D xCamCoords = Vector3D.CrossProduct(up, zCamCoords);
            xCamCoords.Normalize();

            // as zCamCoords and xCamCoords are both normalized and orthogonal the result of cross product is already a normalized vector
            Vector3D yCamCoords = Vector3D.CrossProduct(zCamCoords, xCamCoords);

            // this is row major order!
            this.matLookAt.M11 = xCamCoords.X;
            this.matLookAt.M12 = xCamCoords.Y;
            this.matLookAt.M13 = xCamCoords.Z;

            this.matLookAt.M21 = yCamCoords.X;
            this.matLookAt.M22 = yCamCoords.Y;
            this.matLookAt.M23 = yCamCoords.Z;

            this.matLookAt.M31 = zCamCoords.X;
            this.matLookAt.M32 = zCamCoords.Y;
            this.matLookAt.M33 = zCamCoords.Z;

            this.rotateLookAt.Matrix = this.matLookAt;   // the matrix is always copied, maybe there is a better solution
        }

        #endregion

        #region camera matrix methods

        /// <summary>
        /// The camera matrix defines the transformation for the cameracontrol to get its position and orientation in world space
        /// </summary>
        /// <returns>Matrix3D defining orientation and position of cameracontrol in worldspace</returns>
        public Matrix3D GetCameraMatrix()
        {
            // compose translation and rotation into one matrix
            Matrix3D cameraMatrix = this.matLookAt;
            cameraMatrix.OffsetX = this.translateToPos.OffsetX;
            cameraMatrix.OffsetY = this.translateToPos.OffsetY;
            cameraMatrix.OffsetZ = this.translateToPos.OffsetZ;
            return cameraMatrix;
        }

        /// <summary>
        /// The view matrix transforms points from world space to camera space. It is the invers of the camera matrix.
        /// </summary>
        /// <returns>Matrix3D defining the transformation to camera coordinate system.</returns>
        public Matrix3D GetViewMatrix()
        {
            Matrix3D viewMatrix = this.GetCameraMatrix();
            viewMatrix.Invert();
            return viewMatrix;
        }

        #endregion

        #region private fields

        private TranslateTransform3D translateToPos;
        private MatrixTransform3D rotateLookAt;
        private Matrix3D matLookAt;

        #endregion
    }
}

