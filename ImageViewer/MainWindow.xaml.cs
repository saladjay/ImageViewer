using System;
using System.Collections.Generic;
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
using System.Windows.Forms;

namespace ImageViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        string markedFilePath = string.Empty;
        string imageDir = string.Empty;
        string labelDir = string.Empty;
        IEnumerable<FileInfo> images, labels;
        FileInfo[] image_array, label_array;
        HashSet<string> markStrings = new HashSet<string>();
        int currentIndex = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            images = openDir(out imageDir);
            matchImageAndLabel();
            image_array = images.ToArray();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            labels = openDir(out labelDir);
            matchImageAndLabel();
            label_array = labels.ToArray();
        }

        public IEnumerable<FileInfo> openDir(out string selectedPath)
        {
            selectedPath = string.Empty;
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            //dialog.RootFolder = Environment.SpecialFolder.Programs;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedPath = dialog.SelectedPath;
                DirectoryInfo dinfo = new DirectoryInfo(dialog.SelectedPath);
                return from info in dinfo.GetFiles()
                       orderby info.Name
                       select info;
            }
            else
            {
                return null;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (image_array == null )
            {
                return;
            }
            if (currentIndex >= 0)
            {
                currentIndex = Math.Max(0, currentIndex - 1);
                show(currentIndex);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (image_array == null )
            {
                return;
            }
            if (currentIndex < image_array.Length - 1)
            {
                currentIndex = Math.Min(image_array.Length - 1, currentIndex + 1);
                show(currentIndex);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(markedFilePath))
            {
                var res = System.Windows.MessageBox.Show(messageBoxText: "choose a existed file?", caption: "nothing", button: MessageBoxButton.YesNoCancel);
                var openfiledialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
                if (res == MessageBoxResult.Yes)
                {
                    openfiledialog.IsFolderPicker = false;
                }
                else if (res == MessageBoxResult.No)
                {
                    openfiledialog.IsFolderPicker = true;
                }
                else
                {
                    return;
                }

                if (openfiledialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                {
                    markStrings.Clear();

                    markedFilePath = openfiledialog.FileName;
                    if (openfiledialog.IsFolderPicker)
                    {
                        var result = System.IO.Path.Combine(markedFilePath, $"{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}.txt");
                        if (File.Exists(markedFilePath))
                        {
                            var count = 1;
                            while (true)
                            {
                                result = System.IO.Path.Combine(markedFilePath, $"{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}_{count}.txt");
                                if (File.Exists(result) == false)
                                {
                                    break;
                                }
                                count++;
                            }
                        }
                        markedFilePath = result;
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(markedFilePath))
                        {
                            var lines = sr.ReadToEnd().Split('\r', '\n');
                            foreach (string line in lines)
                            {
                                markStrings.Add(line);
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            if (image_array == null)
            {
                return;
            }
            var image_file = image_array[currentIndex];
            var key = $"{image_file.Name.Substring(0, image_file.Name.Length - image_file.Extension.Length)}";
            if (markStrings.Contains(key))
            {
                return;
            }
            markStrings.Add(key);
            Debug.WriteLine(key);
            StreamWriter sw = new StreamWriter(markedFilePath, true);
            sw.AutoFlush = true;
            sw.WriteLine(key);
            sw.Close();
            mark_count.Text = markStrings.Count.ToString();
            is_marked.Text = "marked";
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Debug.WriteLine(e.Delta);
            var delta = e.Delta / 120;
            currentIndex = Math.Max(0, Math.Min(image_array.Length - 1, currentIndex + delta));
            show(currentIndex);
        }

        public void show(int index)
        {
            while (canvas.Children.Count>1)
            {
                canvas.Children.RemoveAt(1);
            }
            var image_file = image_array[index];
            FileInfo label_file = null;
            if (label_array!=null)
            {
                var label_file_ar =
    from t in label_array
    where image_file.Name.Substring(0, image_file.Name.Length - image_file.Extension.Length).Equals(t.Name.Substring(0, t.Name.Length - t.Extension.Length))
    select t;
                label_file = label_file_ar.FirstOrDefault();
            }

            if (label_file != null)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(image_file.FullName);
                bitmap.EndInit();
                image.Source = bitmap;


                var lines = label_file.OpenText().ReadToEnd().Split('\n', '\r');
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    var nums = line.Split(' ');
                    if (nums.Length != 6)
                    {
                        continue;
                    }
                    var b = new Box(double.Parse(nums[1]), double.Parse(nums[2]), double.Parse(nums[3]), double.Parse(nums[4]), double.Parse(nums[5]), int.Parse(nums[0]));
                    var actually_width = image.ActualWidth;
                    var actually_height = image.ActualHeight;
                    Rectangle rect = new Rectangle();
                    rect.Height = b.Height * actually_height;
                    rect.Width = b.Width * actually_width;
                    rect.StrokeThickness = 2;
                    rect.Stroke = Brushes.Green;
                    Canvas.SetLeft(rect, b.Xmin * actually_width);
                    Canvas.SetTop(rect, b.Ymin * actually_height);
                    canvas.Children.Add(rect);
                    sb.Append(b.ToString());

                    TextBlock tb = new TextBlock();
                    tb.Text = b.Confidence.ToString();
                    tb.Foreground = Brushes.Green;
                    Canvas.SetLeft(tb, b.Xmin * actually_width);
                    Canvas.SetTop(tb, b.Ymin * actually_height);
                    canvas.Children.Add(tb);
                }
                label.Text = sb.ToString();
            }
            else
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(image_file.FullName);
                bitmap.EndInit();
                image.Source = bitmap;

                label.Text = "no file";
            }
            window.Title = image_file.Name;
            var key = $"{image_file.Name.Substring(0, image_file.Name.Length - image_file.Extension.Length)}";
            if (markStrings.Contains(key))
            {
                is_marked.Text = "marked";
            }
            else
            {
                is_marked.Text = "unmarked";
            }
        }


        public void matchImageAndLabel()
        {
            if (images==null)
            {
                return;
            }
            var firstImage = images.First();
            if (labels != null)
            {
                var firstLabel = labels.First();
                if (firstImage.Name.Substring(0, firstImage.Name.Length - firstImage.Extension.Length).Equals(firstLabel.Name.Substring(0, firstLabel.Name.Length - firstLabel.Extension.Length)))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(firstImage.FullName);
                    bitmap.EndInit();
                    image.Source = bitmap;

                    var lines = firstLabel.OpenText().ReadToEnd().Split('\n', '\r');
                    var sb = new StringBuilder();
                    foreach (var line in lines)
                    {
                        var nums = line.Split(' ');
                        if (nums.Length != 6)
                        {
                            continue;
                        }
                        var b = new Box(double.Parse(nums[1]), double.Parse(nums[2]), double.Parse(nums[3]), double.Parse(nums[4]), double.Parse(nums[5]), int.Parse(nums[0]));
                        sb.Append(b.ToString());
                    }
                    label.Text = sb.ToString();
                    
                    
                }

            }
            else
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(firstImage.FullName);
                bitmap.EndInit();
                image.Source = bitmap;

                label.Text = "no file";
            }
            window.Title = firstImage.Name;

        }
    }
    

    public class Box
    {
        public double CenterX { get; private set; }
        public double CenterY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Xmin { get; set; }
        public double Ymin { get; set; }
        public double Xmax { get; set; }
        public double Ymax { get; set; }
        public int Category { get; set; }
        public double Confidence { get; set; }

        public Box(double x,double y,double w,double h,double conf,int cls = 0)
        {
            CenterX = x;
            CenterY = y;
            Width = w;
            Height = h;

            Xmin = x - w / 2.0;
            Ymin = y - h / 2.0;
            Xmax = x + w / 2.0;
            Ymax = y + h / 2.0;

            Confidence = conf;
            Category = cls;
        }

        public override string ToString()
        {
            var sb = $@"box 
xmin:{Xmin.ToString().Substring(0, Math.Min(Xmin.ToString().Length, 5))}
ymin:{Ymin.ToString().Substring(0, Math.Min(Ymin.ToString().Length, 5))}
xmax:{Xmax.ToString().Substring(0, Math.Min(Xmax.ToString().Length, 5))}
ymax:{Ymax.ToString().Substring(0, Math.Min(Ymax.ToString().Length, 5))}
conf:{Confidence.ToString().Substring(0, Math.Min(Confidence.ToString().Length, 5))}
";
            return sb;
        }
    }
}
