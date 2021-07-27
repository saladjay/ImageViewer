using JayCustomControlLib;
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
    [TemplatePart(Name = "TopEdge", Type = typeof(EditedShapeThumb))]
    [TemplatePart(Name = "LeftEdge", Type = typeof(EditedShapeThumb))]
    [TemplatePart(Name = "RightEdge", Type = typeof(EditedShapeThumb))]
    [TemplatePart(Name = "BottomEdge", Type = typeof(EditedShapeThumb))]

    [TemplatePart(Name = "TopMidBox", Type = typeof(EditedShapeThumb))]
    [TemplatePart(Name = "RightMidBox", Type = typeof(EditedShapeThumb))]
    [TemplatePart(Name = "LeftMidBox", Type = typeof(EditedShapeThumb))]
    [TemplatePart(Name = "BottomMidBox", Type = typeof(EditedShapeThumb))]

    [TemplatePart(Name = "TopLeftCornerBox", Type = typeof(EditedShapeThumb))]
    [TemplatePart(Name = "TopRightCornerBox", Type = typeof(EditedShapeThumb))]
    [TemplatePart(Name = "BottomLeftCornerBox", Type = typeof(EditedShapeThumb))]
    [TemplatePart(Name = "BottomRightCornerBox", Type = typeof(EditedShapeThumb))]
    public class BoundingBox : Control
    {
        static BoundingBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BoundingBox), new FrameworkPropertyMetadata(typeof(BoundingBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.GetTemplateChild("TopEdge") is EditedShapeThumb topEdge)
                topEdge.ContentControl = this;

            if (this.GetTemplateChild("LeftEdge") is EditedShapeThumb leftEdge)
                leftEdge.ContentControl = this;

            if (this.GetTemplateChild("RightEdge") is EditedShapeThumb RightEdge)
                RightEdge.ContentControl = this;

            if (this.GetTemplateChild("BottomEdge") is EditedShapeThumb BottomEdge)
                BottomEdge.ContentControl = this;

            if (this.GetTemplateChild("TopMidBox") is EditedShapeThumb topMidEdge)
                topMidEdge.ContentControl = this;

            if (this.GetTemplateChild("RightMidBox") is EditedShapeThumb rightMidEdge)
                rightMidEdge.ContentControl = this;

            if (this.GetTemplateChild("LeftMidBox") is EditedShapeThumb leftMidEdge)
                leftMidEdge.ContentControl = this;

            if (this.GetTemplateChild("BottomMidBox") is EditedShapeThumb bottomMidEdge)
                bottomMidEdge.ContentControl = this;

            if (this.GetTemplateChild("TopLeftCornerBox") is EditedShapeThumb topLeftCornerBox)
                topLeftCornerBox.ContentControl = this;

            if (this.GetTemplateChild("TopRightCornerBox") is EditedShapeThumb topRightCornerBox)
                topRightCornerBox.ContentControl = this;

            if (this.GetTemplateChild("BottomLeftCornerBox") is EditedShapeThumb bottomLeftCornerBox)
                bottomLeftCornerBox.ContentControl = this;

            if (this.GetTemplateChild("BottomRightCornerBox") is EditedShapeThumb bottomRightCornerBox)
                bottomRightCornerBox.ContentControl = this;
        }

    }
}
