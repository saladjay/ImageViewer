using System;
using System.Collections.Generic;
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
            bitmap.UriSource = new Uri(@"E:\rearrange_package_images_and_labels\SentryAndEX\detect_for_SentryAndEX\20210704\exp\00a8d78c390415ccb2fb4e854199499.jpg");
            bitmap.EndInit();
            image.Source = bitmap;
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
            if (sender is Image)
            {
                Image sf = sender as Image;
                if (sf.RenderTransform is ScaleTransform)
                {
                    var st = sf.RenderTransform as ScaleTransform;
                    st.ScaleX = st.ScaleX + 0.1 * delta;
                    st.ScaleY = st.ScaleY + 0.1 * delta;
                    scaleRaito = st.ScaleX;
                }
                else
                {
                    ScaleTransform st = new ScaleTransform();
                    st.ScaleX = 1 + 0.1 * delta;
                    st.ScaleY = 1 + 0.1 * delta;
                    sf.RenderTransform = st;
                    scaleRaito = st.ScaleX;

                }

            }
            foreach (Rectangle rect in canvas.Children)
            {
                if (rect!=null)
                {
                    Canvas.SetTop(rect,Canvas.GetTop())
                }
            }
        }

        Rectangle rect = null;

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition((Image)sender);
            mouseX.Text = position.X.ToString();
            mouseY.Text = position.Y.ToString();
            scaleR.Text = scaleRaito.ToString();
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                rect.Width = position.X * scaleRaito - startWithScale.X;
                rect.Height = position.Y * scaleRaito - startWithScale.Y;
            }

        }

        Point start, end;
        Point startWithScale, endWithScale;
        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            rect = new Rectangle();
            
            rect.StrokeThickness = 3;
            rect.Stroke = Brushes.Red;
            var point = e.GetPosition((Image)sender);
            start = point;
            var x = point.X * scaleRaito;
            var y = point.Y * scaleRaito;
            startWithScale = new Point(x, y);
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            canvas.Children.Add(rect);
        }
    }


    public class BBox : Shape
    {
        protected override Geometry DefiningGeometry
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
