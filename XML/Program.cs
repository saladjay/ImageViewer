using System;
using System.Xml;
using System.Xml.Linq;

namespace XML
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            VOC_XML xml = new VOC_XML(@"E:\bosma-ai\animal_car\animal_car\2008_000008.xml");
            var a = xml.VOC.SelectSingleNode("annotation").SelectNodes("object");
            var b = xml.Path;
            var c = new VOC_XML();
            c.AddInfo("a", "b", "c", "d", "e", 1, 2, 3, 0);
            c.AddSpecialObject("o1", "Unspecified", 0, 0, 11, 22, 33, 44);
            c.Save("a.xml");
        }
    }

    public class VOC_XML
    {
        public VOC_XML()
        {
            VOC = new XmlDocument();
            VOC.AppendChild(VOC.CreateElement("annotation"));
            HasInit = true;
        }

        public VOC_XML(string path)
        {
            VOC = new XmlDocument();
            VOC.Load(path);
            HasInit = true;
        }
#nullable enable

        public XmlDocument? VOC { get; private set; }
        public XDocument VOC_Linq { get; set; }

        private bool _hasInit;

        public bool HasInit
        {
            get { return _hasInit; }
            private set { _hasInit = value; }
        }


        public XmlNode? Annotation => VOC?.SelectSingleNode("annotation");

        public XmlNode? Path => Annotation?.SelectSingleNode("path");

        public XmlNode? FileName => Annotation?.SelectSingleNode("filename");

        public XmlNode? Size => Annotation?.SelectSingleNode("size");

        public XmlNodeList? Objects => Annotation?.SelectNodes("object");

        public XmlNode? GetSpecialObject(int index)
        {
            if (Objects == null)
                return null;
            return Objects[index];
        }



#nullable disable

        public bool AddInfo(string folder, string filename, string path, string source, string device, int width, int height, int depth, int segmented)
        {
            if (Annotation == null)
            {
                return false;
            }
            else
            {
                var folder_node = VOC.CreateElement("folder");
                folder_node.InnerText = folder;
                var filename_node = VOC.CreateElement("filename");
                filename_node.InnerText = filename;
                var path_node = VOC.CreateElement("path");
                path_node.InnerText = path;
                var source_node = VOC.CreateElement("source");
                source_node.InnerText = source;
                var device_node = VOC.CreateElement("device");
                device_node.InnerText = device;
                var segmented_node = VOC.CreateElement("segmented");
                segmented_node.InnerText = segmented.ToString();
                Annotation.AppendChild(folder_node);
                Annotation.AppendChild(filename_node);
                Annotation.AppendChild(path_node);
                Annotation.AppendChild(source_node);
                Annotation.AppendChild(device_node);
                Annotation.AppendChild(BuildSizeNode(width, height, depth));
                Annotation.AppendChild(segmented_node);
                return true;
            }
        }

        private XmlNode BuildSizeNode(int width, int height, int depth)
        {
            var size_node = VOC.CreateElement("size");
            var width_node = VOC.CreateElement("width");
            width_node.InnerText = width.ToString();
            var height_node = VOC.CreateElement("height");
            height_node.InnerText = height.ToString();
            var depth_node = VOC.CreateElement("depth");
            depth_node.InnerText = depth.ToString();
            size_node.AppendChild(width_node);
            size_node.AppendChild(height_node);
            size_node.AppendChild(depth_node);
            return size_node;
        }


        public bool AddSpecialObject(string name, string pose, int truncated, int difficult, int xmin, int ymin, int xmax, int ymax)
        {
            if (Annotation == null)
            {
                return false;
            }
            else
            {
                var obj = VOC.CreateElement("object");
                var name_node = VOC.CreateElement("name");
                name_node.InnerText = name;
                var pose_node = VOC.CreateElement("pose");
                pose_node.InnerText = name;
                var truncated_node = VOC.CreateElement("truncated");
                truncated_node.InnerText = truncated.ToString();
                var difficult_node = VOC.CreateElement("difficult");
                difficult_node.InnerText = difficult.ToString();
                obj.AppendChild(name_node);
                obj.AppendChild(pose_node);
                obj.AppendChild(truncated_node);
                obj.AppendChild(difficult_node);
                obj.AppendChild(BuildBndboxNode(xmin, ymin, xmax, ymax));
                Annotation.AppendChild(obj);
                return true;
            }
        }

        private XmlNode BuildBndboxNode(int xmin, int ymin, int xmax, int ymax)
        {
            var bndbox_node = VOC.CreateElement("bndbox");
            var xmin_node = VOC.CreateElement("xmin");
            xmin_node.InnerText = xmin.ToString();
            var ymin_node = VOC.CreateElement("ymin");
            ymin_node.InnerText = ymin.ToString();
            var xmax_node = VOC.CreateElement("xmax");
            xmin_node.InnerText = xmax.ToString();
            var ymax_node = VOC.CreateElement("ymax");
            ymax_node.InnerText = ymax.ToString();
            bndbox_node.AppendChild(xmin_node);
            bndbox_node.AppendChild(ymin_node);
            bndbox_node.AppendChild(xmax_node);
            bndbox_node.AppendChild(ymax_node);
            return bndbox_node;
        }

        public void Save(string path)
        {
            VOC.Save(path);
        }
    }

}
