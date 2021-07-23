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
            bitmap.UriSource = new Uri(@"C:\Users\Administrator\Desktop\新建文件夹\1.jpg");
            bitmap.EndInit();
            image.Source = bitmap;

            aaa.Data = GenerateMyWeirdGeometry();
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
            //var position = e.GetPosition((Image)sender);
            //if (rect == null)
            //{
            //    return;
            //}
            //rect.SecondPoint = position;
        }

        Point start, end;
        Point startWithScale, endWithScale;

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var bbox = sender as BBox;
            if (bbox!=null)
            {
                bbox.SecondPoint = e.GetPosition(image);
            }
            rect = null;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement item in canvas.Children)
            {
                var rect = item as Rectangle;
                if (rect != null)
                {
                    var list = rect.Tag as List<Point>;
                    if (list != null)
                    {
                        
                    }
                    detectionObject[] nodes = new detectionObject[2];
                    xmlfile aaa = new xmlfile("aaaa.png", new ImageProperty(500, 300, false, new Node("device", "EX", "EX is new device")), nodes);
                    XMLhelper xl = new XMLhelper();
                    xl.AddDeclaration("1.0", "utf-8", "true");
                    xl.BuildXML(aaa);
                    xl.Save("aaaa.xml");

                }
            }
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


    public class BBox : Control
    {


        public bool IsEdit
        {
            get { return (bool)GetValue(IsEditProperty); }
            set { SetValue(IsEditProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEdit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditProperty =
            DependencyProperty.Register("IsEdit", typeof(bool), typeof(BBox), new PropertyMetadata(true));



        public Point FirstPoint
        {
            get { return (Point)GetValue(FirstPointProperty); }
            set { SetValue(FirstPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftTop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstPointProperty =
            DependencyProperty.Register("FirstPoint", typeof(Point), typeof(BBox), new PropertyMetadata(new Point(0, 0), OnFirstPointChanged));

        private static void OnFirstPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BBox bbox)
            {
                if (bbox.SecondPoint.X==0&&bbox.SecondPoint.Y==0)
                {
                    bbox.SecondPoint = bbox.FirstPoint;
                }
                bbox.Width = Math.Abs(bbox.FirstPoint.X - bbox.SecondPoint.X);
                bbox.Height = Math.Abs(bbox.FirstPoint.Y - bbox.SecondPoint.Y);

                Canvas.SetLeft(bbox, Math.Min(bbox.FirstPoint.X, bbox.SecondPoint.X));
                Canvas.SetTop(bbox, Math.Min(bbox.FirstPoint.Y, bbox.SecondPoint.Y));
            }
        }

        public Point SecondPoint
        {
            get { return (Point)GetValue(SecondPointProperty); }
            set { SetValue(SecondPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightBottom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondPointProperty =
            DependencyProperty.Register("SecondPoint", typeof(Point), typeof(BBox), new PropertyMetadata(new Point(0, 0), OnSecondPointPropertyChanged));

        private static void OnSecondPointPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("second");
            Debug.WriteLine(DateTime.Now.Ticks);
            if (d is BBox bbox)
            {
                bbox.Width = Math.Abs(bbox.FirstPoint.X - bbox.SecondPoint.X);
                bbox.Height = Math.Abs(bbox.FirstPoint.Y - bbox.SecondPoint.Y);

                Canvas.SetLeft(bbox, Math.Min(bbox.FirstPoint.X, bbox.SecondPoint.X));
                Canvas.SetTop(bbox, Math.Min(bbox.FirstPoint.Y, bbox.SecondPoint.Y));
            }

        }



        public double RectangleStrokeThickness
        {
            get { return (double)GetValue(RectangleStrokeThicknessProperty); }
            set { SetValue(RectangleStrokeThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeThiness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RectangleStrokeThicknessProperty =
            DependencyProperty.Register("RectangleStrokeThicknessProperty", typeof(double), typeof(BBox), new PropertyMetadata(10));




        public BBox()
        {
            
        }

        Point _moveStartPoint, _oriFirstPoint, _oriSecondPoint;
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (IsEdit==false)
            {
                return;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _moveStartPoint = e.GetPosition((Panel)this.Parent);
                _oriFirstPoint = FirstPoint;
                _oriSecondPoint = SecondPoint;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsEdit == false)
            {
                return;
            }
            if (e.LeftButton== MouseButtonState.Released)
            {
                _oriFirstPoint = FirstPoint;
                _oriSecondPoint = SecondPoint;
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsEdit == false)
            {
                return;
            }
            if (e.LeftButton== MouseButtonState.Pressed)
            {
                var dist = e.GetPosition((Panel)this.Parent) - _moveStartPoint;
                FirstPoint = _oriFirstPoint + dist;
                SecondPoint = _oriSecondPoint + dist;
            }
            base.OnMouseMove(e);
        }

        protected override void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsMouseDirectlyOverChanged(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (e.LeftButton== MouseButtonState.Released)
            {
                this.RectangleStrokeThickness = 2 * RectangleStrokeThickness;
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                this.RectangleStrokeThickness = RectangleStrokeThickness / 2;
            }
            base.OnMouseLeave(e);
        }



        protected  Geometry DefiningGeometry
        {
            get { return GenerateMyWeirdGeometry(); }
        }

        private Geometry GenerateMyWeirdGeometry()
        {
            Debug.WriteLine("geometry");
            Debug.WriteLine(DateTime.Now.Ticks);
            StreamGeometry geom = new StreamGeometry();
            using (StreamGeometryContext gc = geom.Open())
            {
                var diff = SecondPoint - FirstPoint;
                // isFilled = false, isClosed = true
                gc.BeginFigure(new Point(0.0, 0.0), true, true);
                gc.LineTo(new Point(Width, 0), true, true);
                gc.LineTo(new Point(Width, Height), true, true);
                gc.LineTo(new Point(0, Height), true, true);
                gc.LineTo(new Point(0, 0), true, true);
                gc.Close();
                
            }
            //Debug.WriteLine(Width);
            //Debug.WriteLine(Height);
            return geom;
        }


    }
}
