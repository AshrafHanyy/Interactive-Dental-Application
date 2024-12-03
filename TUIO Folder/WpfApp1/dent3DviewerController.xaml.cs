using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace WpfApp1
{
    public partial class dent3DviewerController : UserControl
    {
        private PerspectiveCamera _camera;
        private Transform3DGroup _transformGroup = new Transform3DGroup();
        private double _currentRotationAngle = 0;
        private double _currentVerticalRotationAngle = 0;

        public dent3DviewerController(string filePath, string imagePath)
        {
            InitializeComponent();

            CreateSplitView(filePath, imagePath);
        }

        private void CreateSplitView(string modelFilePath, string imagePath)
        {
            imagePath = @"C:\Users\Administrator\source\repos\Interactive-Dental-Application\TUIO Folder\TUIO11_NET\bin\Debug\Crown Dental APP\2d illustrations\Anterior three quarter crown.png";
            if (File.Exists(modelFilePath))
            {
                LoadSTLModel(modelFilePath);
            }
            else
            {
                MessageBox.Show($"3D model file not found: {modelFilePath}");
            }

            _camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, 300),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 60
            };
            viewport.Camera = _camera;

            if (File.Exists(imagePath))
            {
                imageViewer.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            else
            {
                MessageBox.Show($"Image file not found: {imagePath}");
            }
        }


        private void LoadSTLModel(string filePath)
        {
            try
            {
                var stlReader = new StLReader();
                Model3DGroup model = stlReader.Read(filePath);

                Rect3D bounds = model.Bounds;
                double maxDimension = Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ));

                double scaleFactor = 150 / maxDimension;
                Transform3DGroup transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(new ScaleTransform3D(scaleFactor, scaleFactor, scaleFactor));

                transformGroup.Children.Add(new TranslateTransform3D(
                    -bounds.X - bounds.SizeX / 2,
                    -bounds.Y - bounds.SizeY / 2,
                    -bounds.Z - bounds.SizeZ / 2
                ));

                model.Transform = transformGroup;

                var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
                foreach (var geometry in model.Children)
                {
                    if (geometry is GeometryModel3D geometryModel)
                    {
                        geometryModel.Material = material;
                        geometryModel.BackMaterial = material;
                    }
                }

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
                case "Swipe up":
                    RotateUpDown(5);
                    break;
                case "Swipe down":
                    RotateUpDown(-5);
                    break;
                case "Swipe right":
                    Rotate(5);
                    break;
                case "Swipe left":
                    Rotate(-5);
                    break;
                case "Zoom in":
                    ZoomIn();
                    break;
                case "Zoom out":
                    ZoomOut();
                    break;
            }
        }

        private void Rotate(double degrees)
        {
            Dispatcher.Invoke(() =>
            {
                if (modelVisual.Content != null)
                {
                    _currentRotationAngle += degrees;

                    var rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), degrees);
                    var rotateTransform = new RotateTransform3D(rotation);

                    _transformGroup.Children.Add(rotateTransform);

                    modelVisual.Transform = _transformGroup;
                }

                viewport.InvalidateVisual();
            });
        }

        private void RotateUpDown(double degrees)
        {
            Dispatcher.Invoke(() =>
            {
                if (modelVisual.Content != null)
                {
                    _currentVerticalRotationAngle += degrees;

                    var rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), degrees);
                    var rotateTransform = new RotateTransform3D(rotation);

                    _transformGroup.Children.Add(rotateTransform);

                    modelVisual.Transform = _transformGroup;
                }

                viewport.InvalidateVisual();
            });
        }



        private void ZoomIn()
        {
            Dispatcher.Invoke(() =>
            {
                if (_camera.Position.Z > 100)
                {
                    _camera.Position = new Point3D(_camera.Position.X, _camera.Position.Y, _camera.Position.Z - 50);
                    viewport.InvalidateVisual();
                }
            });
        }

        private void ZoomOut()
        {
            Dispatcher.Invoke(() =>
            {
                if (_camera.Position.Z < 1000)
                {
                    _camera.Position = new Point3D(_camera.Position.X, _camera.Position.Y, _camera.Position.Z + 50);
                    viewport.InvalidateVisual();
                }
            });
        }
    }
}
