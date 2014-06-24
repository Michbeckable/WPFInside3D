using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows;

namespace WPFInside3D
{
    namespace ViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public class CameraControlVM : DependencyObject
        {
            #region properties

            public Transform3D Transform { get; set; }
            public Point3D CamPositionInit { get; set; }
            public Point3D CamLookAtInit { get; set; }

            public CameraControl Control
            {
                get { return this.control; }
                set { }
            }

            #endregion

            #region command properties

            public ICommand camMouseDownCommand
            {
                get { return this.mouseDownCommand; }
            }

            public ICommand camMouseMoveCommand
            {
                get { return this.mouseMoveCommand; }
            }

            public ICommand camMouseUpCommand
            {
                get { return this.mouseUpCommand; }
            }

            public ICommand camMouseWheelCommand
            {
                get { return this.mouseZoomCommand; }
            }

            #endregion

            #region event properties

            /// <summary>
            /// This is done like in the Trackball.cs in the 3DTools toolkit.
            /// </summary>
            private FrameworkElement eventSource;
            public FrameworkElement EventSource
            {
                get { return eventSource; }

                set
                {
                    if (eventSource != null)
                    {
                        eventSource.MouseDown -= this.OnMouseDown;
                        eventSource.MouseUp -= this.OnMouseUp;
                        eventSource.MouseMove -= this.OnMouseMove;
                        eventSource.MouseWheel -= this.OnMouseWheel;
                    }

                    eventSource = value;

                    eventSource.MouseDown += this.OnMouseDown;
                    eventSource.MouseUp += this.OnMouseUp;
                    eventSource.MouseMove += this.OnMouseMove;
                    eventSource.MouseWheel += this.OnMouseWheel;
                }
            }

            #endregion

            #region ctor

            public CameraControlVM()
                : this(new Point3D(0.0, 20.0, 20.0), new Point3D(0.0, 0.0, 0.0))
            {
            }

            public CameraControlVM(Point3D camPosition, Point3D camLookAt)
            {
                this.CamPositionInit = camPosition;
                this.CamLookAtInit = camLookAt;
                this.control = new CameraControl(this.CamPositionInit, this.CamLookAtInit);
                this.Transform = control.Transform;

                this.mouseDownCommand = new DelegateCommand<MouseButtonEventArgs>(
                    (e) => { this.OnMouseDown(null, (MouseButtonEventArgs)e); },
                    (e) => { return true; });

                this.mouseMoveCommand = new DelegateCommand<MouseEventArgs>(
                    (e) => { this.OnMouseMove(null, (MouseEventArgs)e); },
                    (e) => { return true; });

                this.mouseUpCommand = new DelegateCommand<MouseButtonEventArgs>(
                    (e) => { this.OnMouseUp(null, (MouseButtonEventArgs)e); },
                    (e) => { return true; });

                this.mouseZoomCommand = new DelegateCommand<MouseWheelEventArgs>(
                    (e) => { this.OnMouseWheel(null, (MouseWheelEventArgs)e); },
                    (e) => { return true; });
            }

            #endregion

            #region mouse event handling

            private void OnMouseDown(object sender, MouseButtonEventArgs e)
            {
                this.control.OnMouseDown(e);
            }

            private void OnMouseMove(object sender, MouseEventArgs e)
            {
                this.control.OnMouseMove(e);
            }

            private void OnMouseUp(object sender, MouseButtonEventArgs e)
            {
                this.control.OnMouseUp(e);
            }

            private void OnMouseWheel(object sender, MouseWheelEventArgs e)
            {
                this.control.OnMouseWheel(e);
            }

            #endregion

            #region private fields

            private CameraControl control;
            private readonly DelegateCommand<MouseButtonEventArgs> mouseDownCommand;
            private readonly DelegateCommand<MouseEventArgs> mouseMoveCommand;
            private readonly DelegateCommand<MouseButtonEventArgs> mouseUpCommand;
            private readonly DelegateCommand<MouseWheelEventArgs> mouseZoomCommand;

            #endregion
        }

        /// <summary>
        /// template for a command derived from ICommand. 
        /// Taken from http://blog.magnusmontin.net/2013/06/30/handling-events-in-an-mvvm-wpf-application/ .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class DelegateCommand<T> : System.Windows.Input.ICommand
        {
            // these two delegates are called when the command is triggered
            private readonly Predicate<T> _canExecute;
            private readonly Action<T> _execute;

            #region ctor
            public DelegateCommand(Action<T> execute)
                : this(execute, null)
            {
            }

            public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
            {
                _execute = execute;
                _canExecute = canExecute;
            }
            #endregion

            #region ICommand members
            public bool CanExecute(object parameter)
            {
                if (_canExecute == null)
                    return true;

                return _canExecute((parameter == null) ? default(T) : (T)parameter);

                // this does not work as conversion of base types is not possible for MouseEventArgs
                //return _canExecute((parameter == null) ? default(T) : (T)Convert.ChangeType(parameter, typeof(T)));
            }

            public void Execute(object parameter)
            {
                _execute((parameter == null) ? default(T) : (T)parameter);

                // the same issue as in CanExecute
                //_execute((parameter == null) ? default(T) : (T)Convert.ChangeType(parameter, typeof(T)));
            }

            public event EventHandler CanExecuteChanged;
            #endregion

            public void RaiseCanExecuteChanged()
            {
                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
            }
        }

    }
}
