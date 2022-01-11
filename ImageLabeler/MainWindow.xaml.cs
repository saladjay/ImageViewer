using JayCustomControlLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace ImageLabeler
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private double scaleRaito = 1;
        private VOC_XML currentVOC = new VOC_XML();
        
        private struct AnnotatoerConfig
        {
            public string imageName;
            public string VOCXMLFolder;
            public string VOCImageFolder;
            public int imageIndex;
        }

        private struct AnnotatorStatus
        {
            public bool CreatingObject;
            public bool ChangingBBox;
            public bool ChangingCategory;

        }

        private string[] imageFiles = null;
        private string[] labelFiles = null;
        private AnnotatoerConfig config;
        private AnnotatorStatus status;
        private EditedShapeContentControl currentTarget = null;
        private ContextMenu RightButtonMenu = null;
        public MainWindow()
        {
            InitializeComponent();
            InitialAssist();
            if (!string.IsNullOrWhiteSpace(config.VOCImageFolder) && Directory.Exists(config.VOCImageFolder))
            {
                imageFiles = OpenImageDir(config.VOCImageFolder);
                if (imageFiles.Length > 0)
                {
                    LoadImage();
                }
            }
            if (!string.IsNullOrWhiteSpace(config.VOCXMLFolder) && Directory.Exists(config.VOCXMLFolder))
            {
                labelFiles = OpenLabelDir(config.VOCXMLFolder);
                if (labelFiles.Length > 0)
                {
                    LoadLabel();
                }
            }
        }

        public void SaveXML()
        {
            if (currentVOC.IsChanged)
            {
                config.imageIndex = Math.Max(0, Math.Min(config.imageIndex, imageFiles.Length - 1));
                var path = imageFiles[config.imageIndex];
                var imageName = System.IO.Path.GetFileNameWithoutExtension(path);
                var xmlPath = System.IO.Path.Combine(config.VOCXMLFolder, imageName + ".xml");
                currentVOC?.Save(xmlPath);
            }
        }

        public void ReadXML(string path)
        {
            currentVOC.Load(path);
        }

        public string FindImageDir()
        {
            var openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (!string.IsNullOrWhiteSpace(config.VOCImageFolder) && Directory.Exists(config.VOCImageFolder))
            {
                openFolderDialog.SelectedPath = config.VOCImageFolder;
            }
            if(openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                config.VOCImageFolder = openFolderDialog.SelectedPath;
                imageFiles = OpenImageDir(config.VOCImageFolder);
                if (imageFiles.Length > 0)
                {
                    config.imageIndex = 0;
                    LoadImage();
                }
            }
            return null;
        }

        public string[] OpenImageDir(string path)
        {
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".png") || s.EndsWith(".jpg")).ToArray();
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                return null;
            }
        }

        public void FindXMLDir()
        {
            var openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if(!string.IsNullOrWhiteSpace(config.VOCXMLFolder) && Directory.Exists(config.VOCXMLFolder))
            {
                openFolderDialog.SelectedPath = config.VOCXMLFolder;
            }
            if(openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                config.VOCXMLFolder = openFolderDialog.SelectedPath;
                labelFiles = OpenLabelDir(config.VOCXMLFolder);
            }
        }

        public string[] OpenLabelDir(string path)
        {
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                return null;
            }
        }

        public string[] OpenXMLDir(string path)
        {
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    throw;
                }
                return null;
            }
        }

        public void Next()
        {
            while (canvas.Children.Count>4)
            {
                canvas.Children.RemoveAt(canvas.Children.Count - 1);
            }
            if (currentVOC.IsChanged)
            {
                switch (MessageBox.Show("","",MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                    case MessageBoxResult.OK:
                        SaveXML();
                        break;
                    case MessageBoxResult.Cancel:
                    case MessageBoxResult.None:
                    case MessageBoxResult.No:
                    default:
                        break;
                }
            }
            config.imageIndex++;
            LoadImage();
            LoadLabel();
            if (imageFiles != null && imageFiles.Length > 0)
            {
                this.Title = $"{config.imageIndex + 1}/{imageFiles.Count()}";
            }
        }

        public void prev()
        {
            while (canvas.Children.Count > 4)
            {
                canvas.Children.RemoveAt(canvas.Children.Count - 1);
            }
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                UIElement item = canvas.Children[i];
                if (item.Equals(horizonalLine) || item.Equals(verticalLine) || item.Equals(rectangle) || item.Equals(image))
                {
                    continue;
                }
                canvas.Children.RemoveAt(i);
            }
            if (currentVOC.IsChanged)
            {
                switch (MessageBox.Show("", "", MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                    case MessageBoxResult.OK:
                        SaveXML();
                        break;
                    case MessageBoxResult.Cancel:
                    case MessageBoxResult.None:
                    case MessageBoxResult.No:
                    default:
                        break;
                }
            }
            config.imageIndex--;
            LoadImage();
            LoadLabel();
            if (imageFiles != null && imageFiles.Length > 0)
            {
                this.Title = $"{config.imageIndex + 1}/{imageFiles.Count()}";
            }
        }

        private System.Drawing.Bitmap TranslateBitmap(string path)
        {
            System.Drawing.Bitmap bitmap = null;
            using(FileStream fs = new FileStream(path, FileMode.Open))
            {
                System.Drawing.Bitmap ori = new System.Drawing.Bitmap(fs);
                bitmap = new System.Drawing.Bitmap(ori.Width, ori.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(ori, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, ori.Width, ori.Height, System.Drawing.GraphicsUnit.Pixel);
                    g.Dispose();
                }
            }
            return bitmap;
        }

        public void LoadImage()
        {
            if (imageFiles != null && imageFiles.Length > 0)
            {
                config.imageIndex = Math.Max(0, Math.Min(config.imageIndex, imageFiles.Length - 1));
                var path = imageFiles[config.imageIndex];
                var suffix = System.IO.Path.GetExtension(path);
                System.Drawing.Bitmap bitmap = TranslateBitmap(path);
                //bitmap.SetResolution(96, 96);
                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }
                image.Source = bitmapImage;
            }
        }

        public void LoadLabel()
        {
            if (labelFiles != null && labelFiles.Length > 0 && imageFiles != null && imageFiles.Length > 0)
            {
                config.imageIndex = Math.Max(0, Math.Min(config.imageIndex, imageFiles.Length - 1));
                var path = imageFiles[config.imageIndex];
                var imageName = System.IO.Path.GetFileNameWithoutExtension(path);
                var xmlPath = System.IO.Path.Combine(config.VOCXMLFolder, imageName + ".xml");
                currentVOC.Load(xmlPath);
                foreach (XmlNode obj in currentVOC.Objects)
                {
                    var objName = obj.SelectSingleNode("name")?.InnerText;
                    var bndbox = obj.SelectSingleNode("bndbox");
                    var xmin = bndbox.SelectSingleNode("xmin")?.InnerText;
                    var ymin = bndbox.SelectSingleNode("ymin")?.InnerText;
                    var xmax = bndbox.SelectSingleNode("xmax")?.InnerText;
                    var ymax = bndbox.SelectSingleNode("ymax")?.InnerText;
                    PlotBBox(objName, xmin, ymin, xmax, ymax);
                }
            }
        }

        public void PlotBBox(string name, string xminStr, string yminStr, string xmaxStr, string ymaxStr)
        {
            if (int.TryParse(xminStr, out int xmin) &&
                int.TryParse(yminStr, out int ymin) &&
                int.TryParse(xmaxStr, out int xmax) &&
                int.TryParse(ymaxStr, out int ymax))
            {
                PlotBBox(name, xmin, ymin, xmax, ymax);
            }
        }

        public void PlotBBox(string name, int xmin, int ymin, int xmax, int ymax, XmlNode bndbox=null)
        {
            EditedShapeContentControl bbox = new EditedShapeContentControl()
            {
                Width = xmax - xmin,
                Height = ymax - ymin,
                Content = new Label()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = Brushes.Red,
                    ToolTip = name,
                    Opacity = 0.8,
                    Content = name,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    ContextMenu = RightButtonMenu,
                },
                Name = name,
            };
            bbox.GotMouseCapture += Bbox_GotMouseCapture;
            bbox.LostMouseCapture += Bbox_LostMouseCapture;

            bbox.KeyDown += Bbox_KeyDown;
            ((FrameworkElement)bbox.Content).KeyDown += Bbox_KeyDown;
            ((FrameworkElement)bbox.Content).MouseRightButtonDown += Bbox_MouseRightButtonDown;
            bbox.Tag = bndbox;
            canvas.Children.Add(bbox);
            Canvas.SetLeft(bbox, xmin);
            Canvas.SetTop(bbox, ymin);
        }

        private void Bbox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
           
            this.RightButtonMenu.IsOpen = true;
        }

        private void Bbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                int a = 0;
            }
            if (e.Key == Key.S)
            {
                int a = 0;
            }
        }

        private void Bbox_LostMouseCapture(object sender, MouseEventArgs e)
        {
            currentTarget = null;
        }

        private void Bbox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            currentTarget = (EditedShapeContentControl)sender;
        }

        public void ChangeObjectCategory(string name)
        {
            if (currentTarget != null)
            {

            }
        }

        public void StartCreatingObject()
        {
            status.CreatingObject = true;
        }

        public void EndCreatingObject()
        {
            status.CreatingObject = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var workDir = Directory.GetCurrentDirectory();
            string config_str = $"VOCImageFolder:{config.VOCImageFolder}\n" +
                $"VOCXMLFolder:{config.VOCXMLFolder}\n" +
                $"imageIndex:{config.imageIndex}";
          
            File.WriteAllText(System.IO.Path.Combine(workDir, "config.txt"), config_str);
            base.OnClosing(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var workDir = Directory.GetCurrentDirectory();
            var config_path = System.IO.Path.Combine(workDir, "config.txt");
            if (File.Exists(config_path))
            {
                var config_content = File.ReadLines(config_path).ToArray();
                foreach (var line in config_content)
                {
                    if (line.StartsWith("VOCImageFolder"))
                    {
                        config.VOCImageFolder = line.Replace("VOCImageFolder:", "");
                    }
                    if (line.StartsWith("VOCXMLFolder"))
                    {
                        config.VOCXMLFolder = line.Replace("VOCXMLFolder:", "");
                    }
                    if (line.StartsWith("imageIndex"))
                    {
                        if (int.TryParse(line.Replace("imageIndex:", ""), out int value)) {
                            config.imageIndex = value;
                        }
                    }
                }
            }
        }

        private Line verticalLine;
        private Line horizonalLine;
        private Rectangle rectangle;
        private Point startPoint;


        private void InitialAssist()
        {
            if (verticalLine == null)
            {
                verticalLine = new Line()
                {
                    StrokeThickness = 2,
                    X1 = 0,
                    X2 = 2,
                    Y1 = 0,
                    Y2 = image.ActualHeight,
                    Visibility = Visibility.Hidden,
                    Stroke = Brushes.Gray,
                    StrokeDashOffset = 40,
                    IsHitTestVisible = false,
                };
                canvas.Children.Add(verticalLine);
            }
            if (horizonalLine == null)
            {
                horizonalLine = new Line()
                {
                    StrokeThickness = 2,
                    X1 = 0,
                    X2 = image.ActualWidth,
                    Y1 = 0,
                    Y2 = 2,
                    Visibility = Visibility.Hidden,
                    Stroke = Brushes.Gray,
                    StrokeDashOffset = 20,
                    IsHitTestVisible = false,
                };
                canvas.Children.Add(horizonalLine);
            }
            if (rectangle == null)
            {
                rectangle = new Rectangle()
                {
                    Stroke = Brushes.Blue,
                    Fill = Brushes.Blue,
                    Opacity = 0.5,
                    Visibility = Visibility.Hidden,
                    Width = 1,
                    Height = 1,
                    IsHitTestVisible = false,
                };
                canvas.Children.Add(rectangle);
            }
            if (RightButtonMenu == null)
            {
                RightButtonMenu = new ContextMenu() 
                {
                    IsEnabled = true,
                };
                MenuItem delete = new MenuItem()
                {
                    Header = "delete",
                };
                delete.Click += Delete_Click;
                RightButtonMenu.Items.Add(delete);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (status.CreatingObject == false)
            {
                return;
            }
            startPoint = e.GetPosition(canvas);

            verticalLine.Visibility = Visibility.Visible;
            horizonalLine.Visibility = Visibility.Visible;
            rectangle.Width = 1;
            rectangle.Height = 1;
            rectangle.Visibility = Visibility.Visible;
            Canvas.SetLeft(verticalLine, startPoint.X);
            Canvas.SetTop(verticalLine, 0);
            
            Canvas.SetLeft(horizonalLine, 0);
            Canvas.SetTop(horizonalLine, startPoint.Y);

            Canvas.SetLeft(rectangle, startPoint.X);
            Canvas.SetTop(rectangle, startPoint.Y);
        }

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (status.CreatingObject == false)
            {
                return;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition((Image)sender);
                if (verticalLine != null)
                {
                    Canvas.SetLeft(verticalLine, position.X);
                }
                if (horizonalLine != null)
                {
                    Canvas.SetTop(horizonalLine, position.Y);
                }
                if (rectangle != null)
                {
                    rectangle.Width = Math.Abs(startPoint.X - position.X);
                    rectangle.Height = Math.Abs(startPoint.Y - position.Y);
                    Canvas.SetLeft(rectangle, Math.Min(startPoint.X, position.X));
                    Canvas.SetTop(rectangle, Math.Min(startPoint.Y, position.Y));
                }
            }
        }

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (status.CreatingObject == false)
            {
                return;
            }
            if (status.CreatingObject)
            {
                status.CreatingObject = false;
            }
            if (verticalLine != null)
            {
                verticalLine.Visibility = Visibility.Hidden;
            }
            if (horizonalLine != null)
            {
                horizonalLine.Visibility = Visibility.Hidden;
            }
            if (rectangle != null)
            {
                rectangle.Visibility = Visibility.Hidden;
            }
            var endPoint = e.GetPosition(image);
            PlotBBox(name: "", (int)Math.Min(startPoint.X, endPoint.X), (int)Math.Min(startPoint.Y, endPoint.Y), (int)Math.Max(startPoint.X, endPoint.X), (int)Math.Max(startPoint.Y, endPoint.Y));
        }

        private void open_image_dir_Click(object sender, RoutedEventArgs e)
        {
            FindImageDir();
        }

        private void open_label_dir_Click(object sender, RoutedEventArgs e)
        {
            FindXMLDir();
        }

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta / 120;

            if (canvas.RenderTransform is ScaleTransform)
            {
                var st = canvas.RenderTransform as ScaleTransform;
                st.ScaleX = st.ScaleX + 0.1 * delta;
                st.ScaleY = st.ScaleY + 0.1 * delta;
                scaleRaito = st.ScaleX;
            }
            else
            {
                ScaleTransform st = new ScaleTransform();
                st.ScaleX = 1 + 0.1 * delta;
                st.ScaleY = 1 + 0.1 * delta;
                canvas.RenderTransform = st;
                scaleRaito = st.ScaleX;
            }
            return;
        }

        private void Prev_image_Click(object sender, RoutedEventArgs e)
        {
            prev();
        }

        private void Next_image_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Creating_Object_Click(object sender, RoutedEventArgs e)
        {
            status.CreatingObject = true;
        }

        private void Save_Xml_Click(object sender, RoutedEventArgs e)
        {
            SaveXML();
        }
    }
}