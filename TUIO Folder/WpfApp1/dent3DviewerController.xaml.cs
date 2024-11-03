using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace WpfApp1
{
    public partial class dent3DviewerController : UserControl
    {
        private AxisAngleRotation3D rotationAxis;
        private DispatcherTimer timer;

        public dent3DviewerController(string filePath)
        {
            InitializeComponent();
            LoadSTLModel(filePath);
            rotationAxis = (AxisAngleRotation3D)FindName("rotation");

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;
            timer.Start();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //rotationAxis.Angle += 1; 
            //if (rotationAxis.Angle >= 360) rotationAxis.Angle = 0;
        }

        private void LoadSTLModel(string filePath)
        {
            try
            {
                var stlReader = new StLReader();
                Model3DGroup model = stlReader.Read(filePath);

                var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));

                foreach (var geometry in model.Children)
                {
                    if (geometry is GeometryModel3D geometryModel)
                    {
                        geometryModel.Material = material;
                        geometryModel.BackMaterial = material;
                    }
                }

                // Add the model to the viewport
                modelVisual.Content = model;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load STL model: {ex.Message}");
            }
        }
        public void RotateBasedOnCommand(string command)
        {
            if (command == "Swipe right")
            {
                Rotate(90);
            }
            else if (command == "Swipe left")
            {
                Rotate(-90);
            }
        }

        private void Rotate(double degrees)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    rotationAxis.Angle += degrees;

                    if (rotationAxis.Angle >= 360)
                        rotationAxis.Angle -= 360;
                    else if (rotationAxis.Angle < 0)
                        rotationAxis.Angle += 360;

                    viewport.InvalidateVisual();
                    Console.WriteLine("Rotated");
                });
            }

            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while rotating the model: {ex.Message}");
                Console.WriteLine($"Exception Type: {ex.GetType()}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

    }
}
