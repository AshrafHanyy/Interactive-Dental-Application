using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace WpfApp1
{
    public partial class dent3DviewerController : UserControl
    {
        private PerspectiveCamera camera;

        public dent3DviewerController(string filePath)
        {
            InitializeComponent();
            LoadSTLModel(filePath);

            // Initialize the camera
            camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, 500),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 45
            };
            viewport.Camera = camera;
        }

        private void LoadSTLModel(string filePath)
        {
            try
            {
                var stlReader = new StLReader();
                Model3DGroup model = stlReader.Read(filePath);

                // Apply white material to the model
                var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
                foreach (var geometry in model.Children)
                {
                    if (geometry is GeometryModel3D geometryModel)
                    {
                        geometryModel.Material = material;
                        geometryModel.BackMaterial = material;
                    }
                }

                // Initialize a Transform3DGroup to allow cumulative transformations
                Transform3DGroup transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)));
                model.Transform = transformGroup;

                // Set the model content to the viewport
                modelVisual.Content = model;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load STL model: {ex.Message}");
            }
        }

        public void ChangeBasedOnCommand(string command)
        {
            switch (command)
            {
                case "Swipe right":
                    Rotate(90);
                    break;
                case "Swipe left":
                    Rotate(-90);
                    break;
                case "Zoom in":
                    Zoom_in();
                    break;
                case "Zoom out":
                    Zoom_out();
                    break;
            }
        }

        private void Rotate(double degrees)
        {
            Dispatcher.Invoke(() =>
            {
                if (modelVisual.Content.Transform is Transform3DGroup transformGroup)
                {
                    // Clear previous rotations to reset to the base orientation
                    transformGroup.Children.Clear();

                    // Apply a fixed 90-degree rotation on the Y-axis
                    var rotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), degrees));
                    transformGroup.Children.Add(rotation);
                }

                viewport.InvalidateVisual(); // Refresh the viewport to apply the transformation
            });
        }


        private void Zoom_in()
        {
            Dispatcher.Invoke(() =>
            {
                Console.WriteLine("Inside function");
                if (camera.Position.Z > 100)
                {
                    Console.WriteLine("Inside if");
                    camera.Position = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z - 50);
                    viewport.InvalidateVisual();
                }
            });
        }

        private void Zoom_out()
        {
            Dispatcher.Invoke(() =>
            {
                if (camera.Position.Z < 1000)
                {
                    camera.Position = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z + 50);
                    viewport.InvalidateVisual();
                }
            });
        }
    }
}
