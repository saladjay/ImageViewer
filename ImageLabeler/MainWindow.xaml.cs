using JayCustomControlLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ImageLabeler
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private double scaleRaito = 1;

        public MainWindow()
        {
            InitializeComponent();

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(@"E:\package_analysis\ImageViewer\samples\1.jpg");
            bitmap.EndInit();
            image.Source = bitmap;

            //aaa.Data = GenerateMyWeirdGeometry();
        }

        private Geometry GenerateMyWeirdGeometry()
        {
            Debug.WriteLine("geometry");
            Debug.WriteLine(DateTime.Now.Ticks);
            StreamGeometry geom = new StreamGeometry();
            using (StreamGeometryContext gc = geom.Open())
            {
                // isFilled = false, isClosed = true
                gc.BeginFigure(new Point(0.0, 0.0), true, true);
                gc.LineTo(new Point(50, 0), true, true);
                gc.LineTo(new Point(50, 100), true, true);
                gc.LineTo(new Point(0, 100), true, true);
                gc.LineTo(new Point(0, 0), true, true);
                gc.Close();

            }
            //Debug.WriteLine(Width);
            //Debug.WriteLine(Height);
            return geom;
        }

        public void SaveXML()
        {

            xmlfile aaa = new xmlfile("aaaa.png", new ImageProperty(500, 300, false, new Node("device", "EX", "EX is new device")), new detectionObject("horse", false, new bndbox(30, 40, 500, 600, true)), new detectionObject("cat", false, new bndbox(20, 20, 30, 30, true)));
            XMLhelper xl = new XMLhelper();
            xl.AddDeclaration("1.0", "utf-8", "true");
            xl.BuildXML(aaa);
            xl.Save("aaaa.xml");
        }

        public void ReadXML()
        {
            XMLhelper xl = new XMLhelper("aaaa.xml");

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

        BBox rect = null;

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition((Image)sender);
            if (rect == null)
            {
                return;
            }
            rect.SecondPoint = position;
        }

        Point start, end;
        Point startWithScale, endWithScale;

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var bbox = rect as BBox;
            if (bbox != null)
            {
                bbox.SecondPoint = e.GetPosition(image);
            }
            rect = null;
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            rect = new BBox();
            rect.MouseUp += Rect_MouseUp;
            rect.RectangleStrokeThickness = 8;
            //rect.Stroke = Brushes.Red;
            rect.Opacity = 0.5;
            var point = e.GetPosition((Image)sender);

            rect.FirstPoint = point;

            Canvas.SetLeft(rect, point.X);
            Canvas.SetTop(rect, point.Y);
            canvas.Children.Add(rect);
        }

        private void Rect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var bbox = sender as BBox;
            if (bbox != null)
            {
                bbox.SecondPoint = e.GetPosition(image);
            }
            rect = null;
        }
    }
}