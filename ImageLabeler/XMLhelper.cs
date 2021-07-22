using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ImageLabeler
{
    class XMLhelper
    {
        public XDocument Xml
        {
            get
            {
                return xml;
            }

            private set
            {
                xml = value;
            }
        }

        XDocument xml = null;

        private XDeclaration declaration;

        public XDeclaration Declaration
        {
            get { return declaration; }
            set { declaration = value; }
        }


        public XMLhelper(string xmlPath)
        {
            xml = XDocument.Load(xmlPath);
        }

        public XMLhelper()
        {

        }
        
        public void AddDeclaration(string version,string encoding,string standalone)
        {
            declaration = new XDeclaration(version, encoding, standalone);
        }

        public void Save(string path)
        {
            xml.Save(path);
        }

        public void BuildXML(xmlfile content)
        {
            xml = new XDocument(
                Declaration,
                content.ToXElement());
        }
    }

    public class Node:IEnumerable<Node>
    {
        public string Name { get; set; }
        public string Comment { get; set; }

        private string value;
        public string Value
        {
            get { return _nodes.Count > 0 ? null : value; }
            private set { this.value = value; }
        }
    
        public Node()
        {

        }
        public Node(string name, string value, string comment)
        {
            Name = name;
            this.value = value;
            Comment = comment;
        }

        public Node(string name,string value) : this(name, value, null) { }
        public Node(string name) : this(name, string.Empty) { }
        public Node(string name,int value) : this(name, value.ToString()) { }
        public Node(string name,float value) : this(name, value.ToString()) { }

        public Node(string name,Node[] nodes,params Node[] paramNodes)
        {
            this.Name = name;

            if (nodes!=null)
            {
                foreach (Node node in nodes)
                {
                    if (string.IsNullOrWhiteSpace(node.Comment))
                    {
                        _nodes.Add(node);
                    }
                    else
                    {
                        _nodes.Add(new Node(node.Name, node.value));
                        _nodes.Add(node);
                    }
                }
            }
            if (paramNodes != null)
            {
                foreach (Node node in paramNodes)
                {
                    if (string.IsNullOrWhiteSpace(node.Comment))
                    {
                        _nodes.Add(node);
                    }
                    else
                    {
                        _nodes.Add(new Node(node.Name, node.value));
                        _nodes.Add(node);
                    }
                }
            }
        }
        public Node(string name, params Node[] nodes):this(name,nodes,null){ }

        private List<Node> _nodes = new List<Node>();
        public IEnumerator<Node> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public XNode ToXElement()
        {
            if (string.IsNullOrWhiteSpace(value) == false && string.IsNullOrWhiteSpace(Comment) == false)
            {
                return new XComment(Comment);
            }
            return new XElement(Name,Value, (from node in _nodes
                                       orderby string.IsNullOrWhiteSpace(node.Value) ? 0 : 1
                                      select node.ToXElement()).Reverse());
        }
        
    }
    

    public class bndbox : Node
    {
        private Node _xmin, _ymin, _xmax, _ymax;

        public bndbox(Node xmin,Node ymin, Node xmax, Node ymax) : base("bndbox", ymax, xmax, ymin, xmin)
        {
            this._xmin = xmin;
            this._xmin.Name = "xmin";
            this._ymin = ymin;
            this._ymin.Name = "ymin";
            this._xmax = xmax;
            this._xmax.Name = "xmax";
            this._ymax = ymax;
            this._ymax.Name = "ymax";
        }

        public bndbox(int xmin,int ymin,int xmax,int ymax,bool cornerFormat=true):
            this(new Node("xmin",xmin.ToString()),new Node("ymin",ymin.ToString()),new Node("xmax",xmax.ToString()),new Node("ymax",ymax.ToString()))
        { }

        public bndbox(int centerX,int centerY,int width,int height):
            this(xmin:centerX-width/2,ymin:centerY-height/2,xmax:centerX+width/2,ymax: centerY+height/2,cornerFormat: true)
        { }
    }

    public class detectionObject : Node
    {
        private Node _name, _truncated, _bndbox;

        public detectionObject(Node name,Node truncated,bndbox objectBox) : base("object",name, truncated,objectBox)
        {
            _name = name;
            _truncated = truncated;
            _bndbox = objectBox;
            _name.Name = "name";
            _truncated.Name = "truncated";
        }

        public detectionObject(string className,bool isTruncated,bndbox objectBox):
            this(new Node("name",className),new Node("truncated",isTruncated?"1": "0","0 is repersented for object is completed"), objectBox){ }
    }

    public class ImageProperty : Node
    {
        private Node _width, _height, _outdoor;

        public ImageProperty(Node width,Node height,Node outdoor,Node[] nodes) : base("imageProperty",nodes, width, height, outdoor)
        {
            _width = width;
            _width.Name = "width";
            _height = height;
            _height.Name = "height";
            _outdoor = outdoor;
            _outdoor.Name = "outdoor";
        }

        public ImageProperty(int width,int height,bool outdoor) : this(width, height, outdoor, null) { }
        public ImageProperty(int width,int height,bool outdoor,params Node[] nodes):
            this(new Node("width",width.ToString()),new Node("height",height.ToString()),new Node("outdoor",outdoor?"1":"0","1 is represented for outdoor camera"),nodes) { }
    }

    public class xmlfile : Node
    {
        private Node _fileName;
        private ImageProperty _imageProperty;
        private List<detectionObject> _detectionObject;

        public xmlfile(Node fileName, ImageProperty imageProperty, params Node[] nodes) : base("annotation",nodes, fileName, imageProperty)
        {
            _fileName = fileName;
            _fileName.Name = "filename";
            _imageProperty = imageProperty;
            _detectionObject = (from node in nodes
                                where node is detectionObject
                                select (detectionObject)node).ToList();
        }

        public xmlfile(string fileName,ImageProperty imageProperty, params detectionObject[] nodes):
            this(new Node("filename",fileName), imageProperty, nodes) { }
        
    }

    public enum PresetNodeType
    {
        VOC_Object,
    }
}
