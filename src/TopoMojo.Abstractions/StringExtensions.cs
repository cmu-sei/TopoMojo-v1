using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace TopoMojo.Extensions
{
    public static class StringExtensions
    {
        //check for string value
        public static bool HasValue(this string s)
        {
            return (!String.IsNullOrWhiteSpace(s));
        }

        //check for presence of array values
        public static bool IsEmpty(this object[] o)
        {
            return (o == null || o.Length == 0);
        }

        public static bool IsNotEmpty(this object[] o)
        {
            return (o != null && o.Length >0);
        }

        //expands a range string (i.e. [1-3,5,7,10-12]) into an int list
        public static int[] ExpandRange(this string s)
        {
            s = s.Replace("[","").Replace("]","");
            List<int> list = new List<int>();
            string[] sections = s.Split(',');
            foreach (string section in sections)
            {
                //Console.WriteLine(section);
                string[] token = section.Split('-');
                int x = 0, y = 0;
                if (Int32.TryParse(token[0], out x))
                {
                    if (token.Length > 1)
                    {
                        if (!Int32.TryParse(token[1], out y))
                            y = x;
                    }
                    else
                    {
                        y = x;
                    }
                    for (int i = x; i <= y; i++)
                    {
                        //Console.WriteLine(i);
                        list.Add(i);
                    }
                }
            }
            return list.ToArray();
        }

        //extracts string from brackets [].
        public static string Inner(this string s)
        {
            if (s == null)
                s = "";

            int x = s.IndexOf('[');
            if (x > -1)
            {
                int y = s.IndexOf(']', x);
                if (x > -1 && y > -1)
                    s = s.Substring(x + 1, y - x - 1);
            }
            return s.Trim();
        }

        public static string Tag(this string s)
        {
            if (s.HasValue())
            {
                int x = s.IndexOf("#");
                if (x >= 0)
                    return s.Substring(x+1);
            }
            return "";
        }
        //strips hashtag+ from string
        public static string Untagged(this string s)
        {
            if (s.HasValue())
            {
                int x = s.IndexOf("#");
                if (x >= 0)
                    return s.Substring(0, x);
            }
            return s;
        }


        //System.Reflection not fully baked in dotnet core 1.0.0
        // public static void ReplaceString(this object obj, string pattern, string val)
        // {
        //     Type t = obj.GetType();
        //     foreach (PropertyInfo pi in t.GetRuntimeProperties())
        //     {
        //         if (pi.SetMethod != null && pi.PropertyType == typeof(String))
        //             pi.SetValue(obj, pi.GetValue(obj).ToString().Replace(pattern, val));
        //     }
        // }

        // public static object Clone(this object obj)
        // {
        //     Type t = obj.GetType();
        //     Object o = Activator.CreateInstance(t);
        //     foreach (PropertyInfo pi in t.GetRuntimeProperties())
        //     {
        //         if (pi.SetMethod != null && (pi.PropertyType.IsValueType || pi.PropertyType == typeof(String)))
        //             pi.SetValue(o, pi.GetValue(obj));
        //     }
        //     return o;
        // }
    }


}