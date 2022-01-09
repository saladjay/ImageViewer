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
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ImageLabeler"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ImageLabeler;assembly=ImageLabeler"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:BoundingBox/>
    ///
    /// </summary>
    ///     

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
                if (bbox.SecondPoint.X == 0 && bbox.SecondPoint.Y == 0)
                {
                    bbox.SecondPoint = bbox.FirstPoint;
                }
                bbox.Width = Math.Abs(bbox.FirstPoint.X - bbox.SecondPoint.X);
                bbox.Height = Math.Abs(bbox.FirstPoint.Y - bbox.SecondPoint.Y);

                Canvas.SetLeft(bbox, Math.Min(bbox.FirstPoint.X, bbox.SecondPoint.X));
                Canvas.SetTop(bbox, Math.Min(bbox.FirstPoint.Y, bbox.SecondPoint.Y));
                bbox.GenerateMyWeirdGeometry();
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
                bbox.GenerateMyWeirdGeometry();
            }
        }

        public double RectangleStrokeThickness
        {
            get { return (double)GetValue(RectangleStrokeThicknessProperty); }
            set { SetValue(RectangleStrokeThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeThiness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RectangleStrokeThicknessProperty =
            DependencyProperty.Register("RectangleStrokeThicknessProperty", typeof(double), typeof(BBox), new PropertyMetadata(10.0));

        public Geometry BoundingBox
        {
            get { return (Geometry)GetValue(BoundingBoxProperty); }
            private set { SetValue(BoundingBoxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BoundingBox.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoundingBoxProperty =
            DependencyProperty.Register("BoundingBox", typeof(Geometry), typeof(BBox), new PropertyMetadata(default(Geometry)));

        public BBox()
        {

        }

        Point _moveStartPoint, _oriFirstPoint, _oriSecondPoint;
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (IsEdit == false)
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
            if (e.LeftButton == MouseButtonState.Released)
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
            if (e.LeftButton == MouseButtonState.Pressed)
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
            if (e.LeftButton == MouseButtonState.Released)
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

        private void GenerateMyWeirdGeometry()
        {
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
            this.BoundingBox = geom;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.GetTemplateChild("TopEdge") is EditedThumb topEdge)
                topEdge.EditedElement = this;

            if (this.GetTemplateChild("LeftEdge") is EditedThumb leftEdge)
                leftEdge.EditedElement = this;

            if (this.GetTemplateChild("RightEdge") is EditedThumb RightEdge)
                RightEdge.EditedElement = this;

            if (this.GetTemplateChild("BottomEdge") is EditedThumb BottomEdge)
                BottomEdge.EditedElement = this;

            if (this.GetTemplateChild("TopMidBox") is EditedThumb topMidEdge)
                topMidEdge.EditedElement = this;

            if (this.GetTemplateChild("RightMidBox") is EditedThumb rightMidEdge)
                rightMidEdge.EditedElement = this;

            if (this.GetTemplateChild("LeftMidBox") is EditedThumb leftMidEdge)
                leftMidEdge.EditedElement = this;

            if (this.GetTemplateChild("BottomMidBox") is EditedThumb bottomMidEdge)
                bottomMidEdge.EditedElement = this;

            if (this.GetTemplateChild("TopLeftCornerBox") is EditedThumb topLeftCornerBox)
                topLeftCornerBox.EditedElement = this;

            if (this.GetTemplateChild("TopRightCornerBox") is EditedThumb topRightCornerBox)
                topRightCornerBox.EditedElement = this;

            if (this.GetTemplateChild("BottomLeftCornerBox") is EditedThumb bottomLeftCornerBox)
                bottomLeftCornerBox.EditedElement = this;

            if (this.GetTemplateChild("BottomRightCornerBox") is EditedThumb bottomRightCornerBox)
                bottomRightCornerBox.EditedElement = this;
        }

    }
}
