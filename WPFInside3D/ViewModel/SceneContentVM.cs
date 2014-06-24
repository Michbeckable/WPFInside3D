using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPFInside3D
{
    using WPFInside3D.Geometry;

    namespace ViewModel
    {
        /// <summary>
        /// Abstract base class for a 3d scene content model. It exposes the property ContentScene for a ModelVisual3D,
        /// maybe inside a viewport3d, to be bound to. Just derive from this class and override the CreateScene method.
        /// 
        /// See example below how to use this base class.
        /// </summary>
        abstract public class SceneContentVM : DependencyObject
        {
            public SceneContentVM()
            {
                this.Content = new Model3DGroup();
                this.CreateScene();
            }

            public static readonly DependencyProperty ContentProperty =
                DependencyProperty.Register("Content", typeof(Model3DGroup), typeof(SceneContentVM));

            public Model3DGroup Content
            {
                get { return (Model3DGroup)this.GetValue(ContentProperty); }
                set { this.SetValue(ContentProperty, value); }
            }

            abstract protected void CreateScene();
        }

        /// <summary>
        /// Simple implementation of a scene content. Just generating a plane and adding a light.
        /// This is an example use of the MKSceneContentVM class. 
        /// Binding in XAML can be done like this:
        /// 
        /// <Viewport3D>
        ///     <ModelVisual3D Content="{Binding ContentScene}"/> 
        /// 
        /// We assume the DataContext in code behind is set to an instance of this class:
        /// 
        /// this.SceneVM = new MKSceneContentTest();
        /// this.DataContext = this.SceneVM;
        ///         /// </summary>
        public class SceneContentTest : SceneContentVM
        {
            protected override void CreateScene()
            {
                // Add scene geometry
                // just a plane
                //TriangleModel geometry = new Plane(20.0, 20.0);
                //this.Content.Children.Add(geometry.model);

                // and some cubes
                System.Random rnd = new System.Random();
                for (int i = 0; i < 20; i++)
                {
                    double length = 2.0 + (double)rnd.Next(1, 200) / 50;
                    Cube aCube = new Cube(length, length, length);

                    double x = rnd.Next(1, 200) / 10.0 - 10.0;
                    double y = rnd.Next(1, 200) / 20.0 - 5;
                    double z = rnd.Next(1, 200) / 10.0 - 10.0;
                    aCube.model.Transform = new TranslateTransform3D(x, y, z);
                    this.Content.Children.Add(aCube.model);
                }

                // Add the light
                PointLight light = new PointLight(Colors.White, new Point3D(0.0, 10.0, 0.0));
                this.Content.Children.Add(light);
            }
        }
    }
}