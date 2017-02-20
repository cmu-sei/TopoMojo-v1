using System;
using System.Xml;
using System.Xml.Serialization;
using TopoMojo.Extensions;

namespace TopoMojo.vSphere
{
    // public class Vlan
    // {
    //     public int Id { get; set; }
    //     public int Count { get; set; }
    //     public string Net { get; set; }
    // }

    public class DatastorePath
    {
        public DatastorePath(string path)
        {
            if (path.HasValue())
            {
                _folder = path.Replace("\\", "/");
                int x = _folder.IndexOf("[");
                int y = _folder.IndexOf("]");
                if (x >= 0 && y > x)
                {
                    _ds = _folder.Substring(x+1, y-x-1);
                    _folder = _folder.Substring(y+1).Trim();
                }
                x = _folder.LastIndexOf('/');
                _file = _folder.Substring(x+1);
                if (x >= 0)
                {
                    _folder = _folder.Substring(0, x);
                }
                else
                {
                    _folder = "";
                }
            }
        }

        private string _ds;
        public string Datastore
        {
            get { return _ds;}
            set { _ds = value; }
        }

        private string _folder;
        public string FolderPath
        {
            get { return String.Format("[{0}] {1}", _ds, _folder).Trim(); }
            set { _folder = value; }
        }

        public string Folder
        {
            get { return _folder; }
            set { _folder = value; }
        }

        private string _file;
        public string File
        {
            get { return _file;}
            set { _file = value;}
        }

        public void Merge(string path)
        {
            if (path.HasValue())
            {
                string file = "", ds = "", folder = "";
                folder = path.Replace("\\", "/");
                int x = folder.IndexOf("[");
                int y = folder.IndexOf("]");
                if (x >= 0 && y > x)
                {
                    ds = folder.Substring(x+1, y-x-1);
                    folder = folder.Substring(y+1).Trim();
                }
                x = folder.LastIndexOf('/');
                file = folder.Substring(x+1);
                if (x >= 0)
                {
                    folder = folder.Substring(0, x);
                }
                else
                {
                    folder = "";
                }
                Datastore = ds;
                if (Folder.HasValue() && folder.HasValue())
                    folder += "/";
                Folder = folder + Folder;
            }
        }

        public override string ToString()
        {
            string separator = FolderPath.EndsWith("]") ? " " : "/";
            return String.Format("{0}{1}{2}", FolderPath, separator, File);
        }
    }

    public static class Helper
    {
        public static object Clone(object src)
        {
            return Clone(src, src.GetType());
        }
        public static object Clone(object src, Type type)
        {
            //This method offers easy cloning between object types
            //Not optimal, but had trouble with shared wcf data contracts,
            //so found it quickest to have the same object in 2 namespaces and transform them here.

            //instantiate memory stream for serialized object
            using (System.IO.Stream stream = new System.IO.MemoryStream())
            {
                //declare serializers
                XmlSerializer xs, xd;

                //instantiate serializer
                xs = new XmlSerializer(src.GetType());

                //serialize the object
                xs.Serialize(stream, src);

                //set stream to beginning
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                //create serializer of destination type
                xd = (src.GetType() == type) ? xs : new XmlSerializer(type);

                //deserialize object into new type
                return xd.Deserialize(stream);
            }
        }
    }
}