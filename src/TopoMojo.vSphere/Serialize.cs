using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TopoMojo.vSphere
{
    // public static class Helper
    // {
    //     // public static string SerializeToString(object obj)
    //     // {
    //     //     using (MemoryStream ms = new MemoryStream())
    //     //     using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.ASCII))
    //     //     {
    //     //         XmlSerializer ser = new XmlSerializer(obj.GetType());
    //     //         writer.Formatting = Formatting.Indented;
    //     //         ser.Serialize(writer, obj);
    //     //         return Encoding.ASCII.GetString(ms.GetBuffer());
    //     //     }
    //     // }

    //     // public static byte[] Serialize(object obj)
    //     // {
    //     //     using (MemoryStream ms = new MemoryStream())
    //     //     using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.ASCII))
    //     //     {
    //     //         XmlSerializer ser = new XmlSerializer(obj.GetType());
    //     //         writer.Formatting = Formatting.Indented;
    //     //         ser.Serialize(writer, obj);
    //     //         return ms.GetBuffer();
    //     //     }
    //     // }

    //     // public static object Deserialize(Type type, string data)
    //     // {
    //     //     return Deserialize(type, Encoding.ASCII.GetBytes(data));
    //     // }

    //     // public static object Deserialize(Type type, byte[] data)
    //     // {
    //     //     using (MemoryStream ms = new MemoryStream(data))
    //     //     using (XmlTextReader reader = new XmlTextReader(ms))
    //     //     {
    //     //         try
    //     //         {
    //     //             XmlSerializer ser = new XmlSerializer(type);
    //     //             return ser.Deserialize(reader);
    //     //         }
    //     //         catch
    //     //         {
    //     //             return null;
    //     //         }
    //     //     }
    //     // }

    //     public static object Clone(object src)
    //     {
    //         return Clone(src, src.GetType());
    //     }
    //     public static object Clone(object src, Type type)
    //     {
    //         //This method offers easy cloning between object types
    //         //Not optimal, but had trouble with shared wcf data contracts,
    //         //so found it quickest to have the same object in 2 namespaces and transform them here.

    //         //instantiate memory stream for serialized object
    //         using (System.IO.Stream stream = new System.IO.MemoryStream())
    //         {
    //             //declare serializers
    //             XmlSerializer xs, xd;

    //             //instantiate serializer
    //             xs = new XmlSerializer(src.GetType());

    //             //serialize the object
    //             xs.Serialize(stream, src);

    //             //set stream to beginning
    //             stream.Seek(0, System.IO.SeekOrigin.Begin);

    //             //create serializer of destination type
    //             xd = (src.GetType() == type) ? xs : new XmlSerializer(type);

    //             //deserialize object into new type
    //             return xd.Deserialize(stream);
    //         }
    //     }

    // }
}
