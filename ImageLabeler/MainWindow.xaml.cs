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
        private VOC_XML currentVOC = null;
        
        private struct AnnotatoerConfig
        {
            public string imageName;
            public string VOCXMLFolder;
            public string VOCImageFolder;
            public int imageIndex;
        }

        private string[] imageFiles = null;
        private string[] labelFiles = null;
        private AnnotatoerConfig config;

        public MainWindow()
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(config.VOCImageFolder) && Directory.Exists(config.VOCImageFolder))
            {
                imageFiles = OpenImageDir(config.VOCImageFolder);
                if (imageFiles.Length > 0)
                {
                    LoadImage();
                }
            }
        }

        public void SaveXML()
        {

            currentVOC?.Save("");
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
                if (imageFiles.Length>0)
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
            if(!str)
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
            config.imageIndex++;
            LoadImage();
        }

        public void prev()
        {
            config.imageIndex--;
            LoadImage();
        }

        public void LoadImage()
        {
            if(imageFiles!=null && imageFiles.Length > 0)
            {
                config.imageIndex = Math.Max(0, Math.Min(config.imageIndex, imageFiles.Length - 1));
                var path = imageFiles[config.imageIndex];
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                image.Source = bitmap;
            }
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
                        config.VOCXMLFolder = line.Replace("VOCXMLFolder", "");
                    }
                    if (line.StartsWith("imageIndex"))
                    {
                        if(int.TryParse(line.Replace("imageIndex",""), out int value)){
                            config.imageIndex = value;
                        }
                    }
                }
            }
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void open_image_dir_Click(object sender, RoutedEventArgs e)
        {
            FindImageDir();
        }

        private void open_label_dir_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}