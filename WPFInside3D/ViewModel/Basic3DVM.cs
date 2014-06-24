using System;
using System.Windows;

namespace WPFInside3D.ViewModel
{
    using WPFInside3D.Geometry;

    public class Basic3DVM
    {
        public CameraControlVM CameraControl { get; set; }
        public SceneContentTest SceneContent { get; set; }

        public Basic3DVM()
        {
            this.CameraControl = new CameraControlVM();
            this.SceneContent = new SceneContentTest();
        }

        public Basic3DVM(FrameworkElement eventSource)
        {
            this.CameraControl = new CameraControlVM();
            this.CameraControl.EventSource = eventSource;
            
            this.SceneContent = new SceneContentTest();
        }
    }
}
