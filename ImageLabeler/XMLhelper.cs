using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Dynamic;



namespace ImageLabeler
{
    public class xmltoobjectparsertest
    {


        private string _sampleXml = String.Empty;

        public void Setup()
        {
            _sampleXml = @"<?xml version=""1.0"" encoding=""ISO-8859-1""?>
                            <catalog>
                              <cd country=""USA"">
                                <title>Empire Burlesque</title>
                                <artist>Bob Dylan</artist>
                                <price>10.90</price>
                              </cd>
                              <cd country=""UK"">
                                <title>Hide your heart</title>
                                <artist>Bonnie Tyler</artist>
                                <price>10.0</price>
                              </cd>
                              <cd country=""USA"">
                                <title>Greatest Hits</title>
                                <artist>Dolly Parton</artist>
                                <price>9.90</price>
                              </cd>
                            </catalog>";
        }


        public void classic_xml_handling()
        {
            var catalog = new XmlDocument();
            catalog.LoadXml(_sampleXml);

            var numberOfCDsinCatalog = catalog["catalog"].ChildNodes.Count;
            var titleFromUKCD = catalog.SelectSingleNode("/catalog/cd[2]/title").InnerText;


        }


        public void xml_to_object_parsing()
        {
            var catalog = XmlToObjectParser.ParseFromXml(_sampleXml);

            var numberOfCDsinCatalog = catalog.catalog.cd.Count;
            var titleFromUKCD = catalog.catalog.cd[1].title;

        }


        public void can_parse_xml_with_two_equal_elements()
        {
            var xml = @"<xml>
                            <node>FirstEqual</node>
                            <node>SecondEqual</node>
                        </xml>";

            var parsedObject = XmlToObjectParser.ParseFromXml(xml);

        }

    }

    public class XmlToObjectParser
    {
        public static dynamic ParseFromXml(string xml)
        {
            IDictionary<string, object> parsedObject = new ExpandoObject();

            var rootElement = XElement.Parse(xml);

            var root = CreateChildElement(rootElement);

            parsedObject.Add(rootElement.Name.LocalName, root);

            return parsedObject;
        }

        private static dynamic CreateChildElement(XElement parent)
        {
            if (parent.Attributes().Count() == 0 && parent.Elements().Count() == 0)
                return null;

            IDictionary<string, object> child = new ExpandoObject();

            parent.Attributes().ToList().ForEach(attr =>
            {
                child.Add(attr.Name.LocalName, attr.Value);

                if (!child.ContainsKey("NodeName"))
                    child.Add("NodeName", attr.Parent.Name.LocalName);
            });

            parent.Elements().ToList().ForEach(childElement =>
            {
                var grandChild = CreateChildElement(childElement);

                if (grandChild != null)
                {
                    string nodeName = grandChild.NodeName;
                    if (child.ContainsKey(nodeName) && child[nodeName].GetType() != typeof(List<dynamic>))
                    {
                        var firstValue = child[nodeName];
                        child[nodeName] = new List<dynamic>();
                        ((dynamic)child[nodeName]).Add(firstValue);
                        ((dynamic)child[nodeName]).Add(grandChild);
                    }
                    else if (child.ContainsKey(nodeName) && child[nodeName].GetType() == typeof(List<dynamic>))
                    {
                        ((dynamic)child[nodeName]).Add(grandChild);
                    }
                    else
                    {
                        child.Add(childElement.Name.LocalName, CreateChildElement(childElement));
                        if (!child.ContainsKey("NodeName"))
                            child.Add("NodeName", parent.Name.LocalName);
                    }
                }
                else
                {
                    if (child.ContainsKey(childElement.Name.LocalName))
                    {
                        var firstValue = child[childElement.Name.LocalName];
                        child[childElement.Name.LocalName] = new List<dynamic>();
                        ((List<dynamic>)child[childElement.Name.LocalName]).Add(firstValue);
                        ((List<dynamic>)child[childElement.Name.LocalName]).Add(childElement.Value);
                    }
                    else
                    {
                        child.Add(childElement.Name.LocalName, childElement.Value);
                    }

                    if (!child.ContainsKey("NodeName"))
                        child.Add("NodeName", parent.Name.LocalName);
                }
            });

            return child;
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

        public bool Load(string path)
        {
            if (_hasInit && IsChanged)
            {
                return false;
            }
            VOC.Load(path);
            return true;
        }


        public XmlDocument VOC { get; private set; }
        public XDocument VOC_Linq { get; set; }

        private bool _hasInit;

        public bool HasInit
        {
            get { return _hasInit; }
            private set { _hasInit = value; }
        }

        public bool IsChanged { get; private set; }

        public XmlNode Annotation => VOC?.SelectSingleNode("annotation");

        public XmlNode Path => Annotation?.SelectSingleNode("path");

        public XmlNode FileName => Annotation?.SelectSingleNode("filename");

        public XmlNode Size => Annotation?.SelectSingleNode("size");

        public XmlNode Width => Size?.SelectSingleNode("width");

        public XmlNode Height => Size?.SelectSingleNode("height");

        public XmlNode Depth => Size?.SelectSingleNode("depth");

        public XmlNodeList Objects => Annotation?.SelectNodes("object");

        public XmlNode GetSpecialObject(int index)
        {
            if (Objects == null)
                return null;
            return Objects[index];
        }

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
                IsChanged = true;
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
                IsChanged = true;
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

        public void RemoveBndboxNode(XmlNode bndbox)
        {
            Annotation.RemoveChild(bndbox);
            IsChanged = true;
        }

        public void RemoveBndboxNode(string name, int xmin, int ymin, int xmax ,int ymax)
        {
            XmlNode removeNode = null;
            foreach (XmlNode node in Objects)
            {
                var bndbox = node.SelectSingleNode("bndbox");
                if (node.SelectSingleNode("name").InnerText == name
                    && bndbox != null
                    && int.TryParse(bndbox.SelectSingleNode("xmin").InnerText, out int v1) && v1 == xmin
                    && int.TryParse(bndbox.SelectSingleNode("ymin").InnerText, out int v2) && v2 == ymin
                    && int.TryParse(bndbox.SelectSingleNode("xmax").InnerText, out int v3) && v3 == xmax
                    && int.TryParse(bndbox.SelectSingleNode("ymax").InnerText, out int v4) && v4 == ymax)
                {
                    removeNode = node;
                }
            }
            RemoveBndboxNode(removeNode);
        }

        public void Save(string path)
        {
            VOC.Save(path);
            IsChanged = false;
        }
    }
}