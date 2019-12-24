using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace IssueTracker.Helpers
{
    public static class ObjectExtension
    {
        /*
         * Property 값을 가져옴
         * @param obj 객체
         * @param propName Property명
         **/
        public static object GetPropValue(this object obj, string propName)
        {
            Type type = obj.GetType();
            var prop = type.GetProperty(propName);
            if (prop == null) return null;
            return prop.GetValue(obj, null);
        }

        /*
         * Property 값을 설정
         * @param obj 객체
         * @param propName Property명
         * @param value 값
         **/
        public static void SetPropValue(this object obj, string propName, object value)
        {
            Type type = obj.GetType();
            var prop = type.GetProperty(propName);
            if (prop != null) prop.SetValue(obj, value, null);
        }

        public static bool IsNullProp(this PropertyInfo obj, object value)
        {
            bool isnull = false;
            try{
                object o = obj.GetValue(value, null);
                if(o == null){
                    isnull = true;
                }
            }catch (Exception){
                isnull  = true;
            }
            return isnull;
        }

        /*
         * 객체내부를 덤프
         * @param obj 객체
         * @param nl 계행문자
         **/
        public static string Dump(this object obj) { return Dump(obj, "\n"); }
        public static string Dump(this object obj, string nl)
        {
            if (obj == null) return "";
            StringBuilder sb = new StringBuilder();
            int index = 0;
            if (obj is IList) {
                foreach (object o in (IList)obj) {
                    Util.SB(sb, "IList[", index, "]=>", Dump(o, nl));
                    index++;
                }
            } else if (obj is Array) {
                foreach (object o in (Array)obj) {
                    Util.SB(sb, "Array[", index, "]=>", Dump(o, nl));
                    index++;
                }
            } else if (obj is IDictionary) {
                foreach (DictionaryEntry entry in (IDictionary)obj) {
                    Util.SB(sb, "Dic[", Util.S(entry.Key), "]=>", Dump(entry.Value, nl));
                }
            } else if (obj is NameValueCollection) {
                foreach (string col in ((NameValueCollection)obj).Keys) {
                    Util.SB(sb, "NVC[", col, "]=>", Dump(((NameValueCollection)obj)[col], nl));
                }
            } else if (obj is string) {
                Util.SB(sb, obj, nl);
            } else {
                Type type = obj.GetType();
                if (type.IsValueType) {
                    Util.SB(sb, Util.S(obj), nl);
                } else {
                    foreach (var prop in type.GetProperties()) {
                        Util.SB(sb, "{", prop.Name, "}=>", prop.GetValue(obj, null), nl);
                    }
                }
            }
            return sb.ToString();
        }

    }
}
