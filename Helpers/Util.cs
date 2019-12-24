using System;
using System.Configuration;
using System.Web;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections;
using System.Linq;
using System.Xml;
using NVC = System.Collections.Specialized.NameValueCollection;
using HT = System.Collections.Hashtable;
using System.Linq.Expressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Helpers
{
    // 각종 Util성 Helper Function모음
    public class Util
    {
        public static ILogger _logger;

        /* ht, nvc (Hashtable, NameValueCollection)
         * ----------------------------------------------------------------------------- */
        public static HT NewHT(params object[] items)
        {
            // NewHT(Key1, Value1, Key2, Value2, ...)
            HT ht = new HT();
            for (int i = 0; i < items.Length; i += 2) {
                if (i + 1 < items.Length) {
                    ht.Add(items[i], items[i + 1]);
                }
            }
            return ht;
        }
        public static HT AddHT(HT ht1, params object[] items)
        {
            // AddHT(Key1, Value1, Key2, Value2, ...)
            HT ht = (ht1 == null) ?
                ht = new HT()
                :
                ht = new HT(ht1);
            for (int i = 0; i < items.Length; i += 2) {
                if (i + 1 < items.Length) {
                    ht.Add(items[i], items[i + 1]);
                }
            }
            return ht;
        }
        public static object HT(HT ht, Array keys)
        {
            HT last = ht;
            int index = 0;
            foreach (object key in keys) {
                if (last[key] is HT) {
                    last = last[key] as HT;
                } else {
                    if (index == keys.Length - 1) {
                        return last[key];
                    }
                }
                index++;
            }
            return null;
        }
        public static void HT(HT ht, Array keys, object val)
        {
            HT last = ht;
            int index = 0;
            foreach (object key in keys) {
                if (!last.Contains(key)) {
                    last.Add(key, null);
                }
                if (index == keys.Length - 1) {
                    last[key] = val;
                } else {
                    if (last[key] == null) {
                        last[key] = new HT();
                    }
                    last = last[key] as HT;
                }
                index++;
            }
        }

        public static HT ParseHT(string data) { return ParseHT(data, null); }
        public static HT ParseHT(string data, HT ht1)
        {
            // key1:val1,key2:val2,key3:val3, ...
            HT ht = (ht1 != null) ? new HT(ht1) : new HT();
            _logger.LogDebug("Util.ParseHT : " + data);
			string[] items = data.Split(',');
			for (int i = 0; i < items.Length; i++) {
				if (string.IsNullOrEmpty(items[i])) continue;
                var pos = items[i].IndexOf(':');
                if (pos==-1) {
					ht.Add(i, items[i]);
                } else {
                    string key = items[i].Substring(0, pos);
                    string value = items[i].Substring(pos+1);
					ht.Add(key, value);
                }
			}
			return ht;
        }
        public static HT ExtendHT(HT child, HT parent) { return ExtendHT(child, parent, true); }
        public static HT ExtendHT(HT child, HT parent, bool bNewAdd)
        {
            if (parent == null) return child;
            foreach (object key in parent.Keys) {
                if (child.Contains(key)) {
                    child[key] = parent[key];
                } else if (bNewAdd) {
                    child.Add(key, parent[key]);
                }
            }
            return child;
        }
        public static NVC NewNVC(params string[] items)
        {
            // NewNVC(Key1, Value1, Key2, Value2, ...)
            NVC nvc = new NVC();
            for (int i = 0; i < items.Length; i += 2) {
                if (i + 1 < items.Length) {
                    nvc.Add(items[i], items[i + 1]);
                }
            }
            return nvc;
        }
        public static NVC AddNVC(NVC nvc1, params string[] items)
        {
            // AddNVC(Key1, Value1, Key2, Value2, ...)
            NVC nvc = new NVC(nvc1);
            for (int i = 0; i < items.Length; i += 2) {
                if (i + 1 < items.Length) {
                    nvc.Add(items[i], items[i + 1]);
                }
            }
            return nvc;
        }
        public static NVC ParseNVC(string data) { return ParseNVC(data, null); }
        public static NVC ParseNVC(string data, NVC nvc1)
        {
            // key1:val1,key2:val2,key3:val3, ...
            NVC nvc = (nvc1 != null) ? new NVC(nvc1) : new NVC();
            string[] items = data.Split(',', ':');
            for (int i = 0; i < items.Length; i += 2) {
                if (i + 1 < items.Length) {
                    nvc.Add(items[i], items[i + 1]);
                }
            }
            return nvc;
        }

        public static NVC ExtendNVC(NVC child, NVC parent) { return ExtendNVC(child, parent, true); }
        public static NVC ExtendNVC(NVC child, NVC parent, bool bNewAdd)
        {
            if (parent == null) return child;
            foreach (string key in parent.Keys) {
                if (!string.IsNullOrEmpty(child[key])) {
                    child[key] = parent[key];
                } else if (bNewAdd) {
                    child.Add(key, parent[key]);
                }
            }
            return child;
        }
        public static ArrayList SortedHT(HT ht)
        {
            ArrayList sorter = new ArrayList();
            sorter.AddRange(ht);
            sorter.Sort(new htSortedClass());
            return sorter;
        }
        private class htSortedClass : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                if (x == null) return -1;
                if (y == null) return 1;
                HT ht1 = ((DictionaryEntry)x).Value as HT;
                HT ht2 = ((DictionaryEntry)y).Value as HT;
                if (ht1 == null && ht2 == null) return 0;
                if (ht1 == null) return -1;
                if (ht2 == null) return 1;
                int n1 = ToInt(ht1["_order"]);
                int n2 = ToInt(ht2["_order"]);
                return (int)(n1 - n2);
            }
        }

        public static HT QueryToHT(string query)
        {
            HT ht = new HT();
            var groups = query.Split('&');
            foreach (var group in groups) {
                if (string.IsNullOrEmpty(group)) continue;
                var items = group.Split('=');
                if (items.Length < 2) continue;
                ht[items[0]] = items[1];
            }
            return ht;
        }

        public static string HTToQuery(HT query)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DictionaryEntry entry in query) {
                if (sb.Length > 0) {
                    Util.SB(sb, "&");
                }
                Util.SB(sb, entry.Key, "=");
                Util.SB(sb, entry.Value);
            }
            return sb.ToString();
        }

        /* alias
         * ----------------------------------------------------------------------------- */
        public static object[] A(params object[] items)
        {
            return items;
        }
        public static string S(object str)
        {
            if (str == null) str = "";
            return str.ToString();
        }
        public static string F(string format, params object[] args)
        {
            return string.Format(format, args);
        }
        public static string SB(params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object s in args) {
                sb.Append(s);
            }
            return sb.ToString();
        }
        public static void SB(StringBuilder sb, params object[] args)
        {
            foreach (object s in args) {
                sb.Append(s);
            }
        }
        public static string Substring(object str, int start)
        {
            if (str == null) str = "";
            string text = str.ToString();
            if (start > text.Length) start = text.Length;
            if (start < 0) start = text.Length + start;
            return text.Substring(start);
        }
        public static string Substring(object str, int start, int length)
        {
            if (str == null) str = "";
            string text = str.ToString();
            if (start > text.Length) start = text.Length;
            if (start < 0) start = text.Length + start;
            if (start + length > text.Length) {
                length = text.Length - start;
            }
            return text.Substring(start, length);
        }

        public static string Substring(object str, string startSymbol, string endSymbol)
        {
            if (str == null) str = "";
            string text = str.ToString();
            int start = 0;
            int end = 0;
            int resultLen = text.Length;
            if (text.IndexOf(startSymbol) >= 0) {
                start = text.IndexOf(startSymbol) + 1;
            }
            if (text.IndexOf(endSymbol) > 0 && endSymbol != "") {
                end = text.IndexOf(endSymbol,start);
            } else {
                end = text.Length;
            }
            resultLen = end - start;

            return text.Substring(start, resultLen);
        }

        public static string SubstringByByte(object str, int count)
        {
            try {
                if (str == null) str = "";
                string text = str.ToString();
                byte[] strByte = Encoding.Default.GetBytes(text);

                if (strByte.Count() > count) {
                    char[] cc = Encoding.Default.GetChars(strByte, 0, count);
                    return string.Join("", cc);
                } else {
                    return text;
                }
            } catch {
                return "";
            }
        }

        public static string Pick(string val, string def)
        {
            if (string.IsNullOrEmpty(val)) return def;
            return val;
        }
        public static string Pick(object val, string def)
        {
            if (val==null) return def;
            return val.ToString();
        }
        public static object Pick(object val, object def)
        {
            if (val==null) return def;
            return val.ToString();
        }

        /* string 
         * ----------------------------------------------------------------------------- */
        public static bool IsNullOrEmpty(object val)
        {
            return string.IsNullOrEmpty(S(val));
        }
        public static string ReplaceOnce(string str, string oldVal, string newVal) { return ReplaceOnce(str, oldVal, newVal, StringComparison.CurrentCulture); }
        public static string ReplaceOnce(string str, string oldVal, string newVal, StringComparison comp)
        {
            string newstr = str.TrimStart();
            int spos = newstr.IndexOf(newstr, comp);
            if (spos != -1) {
                newstr = newstr.Remove(spos, oldVal.Length).Insert(spos, newVal);
            }
            return newstr;
        }

        /*
         * String.Format 확장
         * index가 아닌 정확한 명칭 매칭
         * ex) 
         * string text = "이름은 {@이름}이고 연락처는 {@연락처}입니다.";
         * Util.Format(text, new HT {{"이름", "홍길동"},{"연락처", "02-1234-1234"}})
         */
        public static string Format(string str, HT map)
        {
            string str1 = str;
            int ctlpos = 0;
            Regex regex = new Regex(@"\{@([^\}]*)\}");
            MatchCollection mc = regex.Matches(str1);
            foreach (Match match in mc) {
                if (match.Groups.Count >= 2) {
                    string name = match.Groups[1].Value;
                    int pos = match.Groups[0].Index + ctlpos;
                    int len = match.Groups[0].Length;
                    if (map.Contains(name)) {
                        str1 = str1.Remove(pos, len)
                            .Insert(pos, S(map[name]));
                        ctlpos += S(map[name]).Length - len;
                    } else {
                        str1 = str1.Remove(pos, len);
                        ctlpos -= len;
                    }
                }
            }
            return str1;
        }
        public static string Format(string str, DataRow map)
        {
            string str1 = str;
            int ctlpos = 0;
            Regex regex = new Regex(@"\{@([^\}]*)\}");
            MatchCollection mc = regex.Matches(str1);
            foreach (Match match in mc) {
                if (match.Groups.Count >= 2) {
                    string name = match.Groups[1].Value;
                    int pos = match.Groups[0].Index + ctlpos;
                    int len = match.Groups[0].Length;
                    if (map.Table.Columns.Contains(name) && map[name]!=null) {
                        str1 = str1.Remove(pos, len)
                            .Insert(pos, S(map[name]));
                        ctlpos += S(map[name]).Length - len;
                    } else {
                        str1 = str1.Remove(pos, len);
                        ctlpos -= len;
                    }
                }
            }
            return str1;
        }
        /*
         * Util.Format의 확장
         * 대체값에 대한 if 비교
         * ex) 
         * string text = "이름은 {@이름} {?연락처}이고 연락처는 {@연락처}입니다.{/연락처}{?!연락처}입니다.{/연락처}";
         * Util.Format(text, new HT {{"이름", "홍길동"}})
         * 연락처가 없으므로 뒷부분은 "입니다"로 치환
         * text => 이름은 홍길동입니다.
         */
        public static string FormatEx(string str, HT map)
        {
            string str1 = str;
            int ctlpos = 0;
            Regex regex = new Regex(@"\{\?([\!]?)([^\}]*)\}(.+?)\{\/\2\}", RegexOptions.Singleline);
            MatchCollection mc = regex.Matches(str1);
            foreach (Match match in mc) {
                if (match.Groups.Count >= 3) {
                    string not = match.Groups[1].Value;
                    string name = match.Groups[2].Value;
                    string content = match.Groups[3].Value;
                    int pos = match.Groups[0].Index + ctlpos;
                    int len = match.Groups[0].Length;
                    if (map.Contains(name) && map[name]!=null) {
                        if (string.IsNullOrEmpty(not)) {
                            content = FormatEx(content, map);
                            str1 = str1.Remove(pos, len);
                            str1 = str1.Insert(pos, content);
                            ctlpos += content.Length - len;
                        } else {
                            str1 = str1.Remove(pos, len);
                            ctlpos -= len;
                        }
                    } else {
                        if (!string.IsNullOrEmpty(not)) {
                            content = FormatEx(content, map);
                            str1 = str1.Remove(pos, len);
                            str1 = str1.Insert(pos, content);
                            ctlpos += content.Length - len;
                        } else {
                            str1 = str1.Remove(pos, len);
                            ctlpos -= len;
                        }
                    }
                }
            }
            return Format(str1, map);
        }
        public static string FormatEx(string str, DataRow map)
        {
            string str1 = str;
            int ctlpos = 0;
            Regex regex = new Regex(@"\{\?([\!]?)([^\}]*)\}(.+?)\{\/\2\}", RegexOptions.Singleline);
            MatchCollection mc = regex.Matches(str1);
            foreach (Match match in mc) {
                if (match.Groups.Count >= 3) {
                    string not = match.Groups[1].Value;
                    string name = match.Groups[2].Value;
                    string content = match.Groups[3].Value;
                    int pos = match.Groups[0].Index + ctlpos;
                    int len = match.Groups[0].Length;
                    if (map.Table.Columns.Contains(name) && map[name]!=null) {
                        if (string.IsNullOrEmpty(not)) {
                            content = FormatEx(content, map);
                            str1 = str1.Remove(pos, len);
                            str1 = str1.Insert(pos, content);
                            ctlpos += content.Length - len;
                        } else {
                            str1 = str1.Remove(pos, len);
                            ctlpos -= len;
                        }
                    } else {
                        if (!string.IsNullOrEmpty(not)) {
                            content = FormatEx(content, map);
                            str1 = str1.Remove(pos, len);
                            str1 = str1.Insert(pos, content);
                            ctlpos += content.Length - len;
                        } else {
                            str1 = str1.Remove(pos, len);
                            ctlpos -= len;
                        }
                    }
                }
            }
            return Format(str1, map);
        }
        /*
         * 연속 포맷 정의
         * text = "1111112222222"
         * SubFormat(text, 6, "-", 7); 
         * => 111111-2222222
         * text = "0212345678"
         * SubFormat(text, 2, "-", 4, "-", 4); 
         * => 02-1234-5678
         **/
        public static string SubFormat(string str, params object[] paramlist)
        {
            StringBuilder sb = new StringBuilder();
            int totalpos = 0;
            int? startlen = null;
            int? length = null;
            foreach (object param in paramlist) {
                if (param is int) {
                    if (startlen == null) {
                        startlen = (int)param;
                    } else if (length == null) {
                        length = (int)param;
                    } else {
                        sb.Append(str.Substring((int)startlen, (int)length));
                        startlen = null;
                        length = null;
                    }
                } else {
                    if (startlen != null && length != null) {
                        sb.Append(str.Substring((int)startlen, (int)length));
                        sb.Append(S(param));
                        totalpos = (int)startlen + (int)length;
                    } else if (startlen != null) {
                        sb.Append(str.Substring(totalpos, (int)startlen));
                        sb.Append(S(param));
                        totalpos += (int)startlen;
                    }
                    startlen = null;
                    length = null;
                }
            }
            if (startlen != null && length != null) {
                sb.Append(str.Substring((int)startlen, (int)length));
            } else if (startlen != null) {
                sb.Append(str.Substring(totalpos, (int)startlen));
            }

            return sb.ToString();
        }
        public static string Trim(object str)
        {
            return str.ToString().Trim();
        }
        public static string DBString(string str)
        {
            return str.Replace("--", "").Replace("'", "''");
        }

        /**
         * 텍스트를 일정 길이 만큼 자르는 함수
         *
         * @param string str 텍스트
         * @param string len 자를 길이
         * @param string suffix 끝에 붙일 텍스트
         * @return string 잘린 텍스트
         */
        public static string CutStr(string str, int len, string suffix = "..")
        {
            System.Text.Encoding myEncoding = System.Text.Encoding.GetEncoding("ks_c_5601-1987");

            byte[] buf = myEncoding.GetBytes(str);
            if (buf.Length < len) return str;

            string result = myEncoding.GetString(buf, 0, len);
            if (result.Substring(result.Length - 1) == "?") {
                result = result.Substring(0, result.Length - 1);
            }

            return result + suffix;
        }


        /* request (post in server.execute)
         * ----------------------------------------------------------------------------- */
        public static void ServerRequest(StringWriter output, string url, NVC post)
        {
        }

        /* convert helper
         * ----------------------------------------------------------------------------- */
        public static string ToPrice(object prc)
        {
            if (prc==null) return "";
            if (prc is Int32) {
                return ((Int32)prc).ToString("N0");
            } else if (prc is Int64) {
                return ((Int64)prc).ToString("N0");
            } else {
                Int64 n = 0;
                try {
                    string s = prc.ToString();
                    int pos = s.LastIndexOf('.');
                    if (pos==-1) {
                        s = Regex.Replace(s, "[^-0-9]", "");
                        n = Int64.Parse(s);
                        return n.ToString("N0");
                    } else {
                        var num = Int64.Parse(Regex.Replace(s.Substring(0, pos), "[^-0-9]", ""));
                        var frac = s.Substring(pos + 1);
                        if (string.IsNullOrEmpty(frac)) {
                            return num.ToString("N0");
                        } else {
                            return num.ToString("N0") + "." + frac;
                        }
                    }
                } catch {}
                return prc.ToString();
            }
        }
        public static int ParsePriceVal(object str)
        {
            string price = S(str).Replace("\\", "").Replace(",", "");
            return ToInt(price);
        }
        public static string ParsePrice(object str)
        {
            string price = S(str).Replace("\\", "").Replace(",", "");
            return price;
        }
      
        public static int ToInt(object str) { return ToInt(str, 0); }
        public static int ToInt(object str, int def)
        {
            if (str is int) return (int)str;
            int n = 0;
            try {
                str = ParsePrice(str);
                n = int.Parse(S(str));
            } catch { n = def; }
            return n;
        }

        public static long ToLong(object str) { return ToInt(str, 0); }
        public static long ToLong(object str, long def)
        {
            if (str is long) return (long)str;
            long n = 0;
            try {
                str = ParsePrice(str);
                n = long.Parse(S(str));
            } catch { n = def; }
            return n;
        }
        public static double ToDouble(object str) { return ToDouble(str, 0.0); }
        public static double ToDouble(object str, double def)
        {
            if (str is double) return (double)str;
            if (str is float) return double.Parse(str.ToString());
			double n = 0;
            try {
                str = ParsePrice(str);
                n = double.Parse(S(str));
            } catch { n = def; }
            return n;
        }
        public static bool ToBool(object str)
        {
            if (str is bool) {
                return (bool)str;
            } else if (str is int) {
                return (int)str != 0;
            }
            bool ret = false;
            try {
                ret = bool.Parse(S(str));
            } catch {
                ret = (ToInt(str) != 0);
            }
            return ret;
        }
        public static string ToJson(object obj)
        {
            if (obj == null) return "{}";
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }
        public static string ToTitleCase(string str)
        {
            TextInfo txtinfo = CultureInfo.CurrentCulture.TextInfo;
            return txtinfo.ToTitleCase(str);
        }
        public static string ToClassCase(string str)
        {
            Regex regex = new Regex(@"([A-Z])");
            Regex regexDiv = new Regex(@"[^a-zA-Z0-9]");
            string clsstr = regex.Replace(str, " $1");
            TextInfo txtinfo = CultureInfo.CurrentCulture.TextInfo;
            string[] words = regexDiv.Split(clsstr);
            StringBuilder builder = new StringBuilder();
            foreach (string word in words) {
                if (word.Length == 0) continue;
                builder.Append(txtinfo.ToTitleCase(word));
            }
            return builder.ToString();
        }
        public static string ToCamelCase(string str)
        {
            Regex regex = new Regex(@"([A-Z])");
            Regex regexDiv = new Regex(@"[^a-zA-Z0-9]");
            string camelstr = regex.Replace(str, " $1");
            TextInfo txtinfo = CultureInfo.CurrentCulture.TextInfo;
            string[] words = regexDiv.Split(camelstr);
            StringBuilder builder = new StringBuilder();
            foreach (string word in words) {
                if (word.Length == 0) continue;
                if (builder.Length > 0) {
                    builder.Append(txtinfo.ToTitleCase(word));
                } else {
                    builder.Append(word.ToLower());
                }
            }
            return builder.ToString();
        }
        public static string ToUnderbarCase(string str)
        {
            Regex regex = new Regex(@"([A-Z])");
            Regex regexDiv = new Regex(@"[^a-zA-Z0-9]");
            string understr = regex.Replace(str, " $1");
            string[] words = regexDiv.Split(understr);
            StringBuilder builder = new StringBuilder();
            foreach (string word in words) {
                if (word.Length == 0) continue;
                if (builder.Length > 0) builder.Append("_");
                builder.Append(word.ToLower());
            }
            return builder.ToString();
        }
        public static string ToShortDate(object obj)
        {
            return ToDate(obj, "yyyy-MM-dd");
        }
        public static string ToDate(object obj)
        {
            return ToDate(obj, "yyyy-MM-dd hh:mm:ss");
        }
        public static string ToDate(object obj, string format)
        {
            string date = "";
            try {
                if (obj is DateTime) {
                    date = ((DateTime)obj).ToString(format);
                } else {
                    date = S(obj);
                    if (string.IsNullOrEmpty(date)) return "";

                    if (date.Length > 10) {
                        date = Substring(date, 0, 10);
                    }

                    date = DateTime.Parse(date).ToString(format);
                }
            } catch (Exception ex) {
                _logger.LogError("Util:ToDate " + obj, ex);
            }
            return date;
        }
        public static NVC ToNVC(DataRowCollection rows, string key, string val)
        {
            NVC nvc = new NVC();
            if (rows != null) {
                foreach (DataRow row in rows) {
                    nvc.Add(row[key].ToString(), row[val].ToString());
                }
            }
            return nvc;
        }
        public static NVC ToNVC(DataRow row)
        {
            NVC nvc = new NVC();
            foreach (DataColumn col in row.Table.Columns) {
                nvc.Add(col.ToString(), row[col].ToString());
            }
            return nvc;
        }
        public static NVC ToNVC(HT ht)
        {
            NVC nvc = new NVC();
            foreach (object col in ht.Keys) {
                if (col == null) continue;
                nvc.Add(col.ToString(), (ht[col] ?? "").ToString());
            }
            return nvc;
        }
        public static HT ToHT(DataRowCollection rows, string key, string val)
        {
            HT ht = new HT();
            if (rows != null) {
                foreach (DataRow row in rows) {
                    ht.Add(row[key].ToString(), row[val].ToString());
                }
            }
            return ht;
        }
        public static HT ToHT(DataRow row)
        {
            HT ht = new HT();
            foreach (DataColumn col in row.Table.Columns) {
                ht.Add(col.ToString(), row[col]);
            }
            return ht;
        }
        public static HT ToHT(NVC nvc)
        {
            HT ht = new HT();
            foreach (string key in nvc.Keys) {
                if (key == null) continue;
                ht.Add(key, nvc[key]);
            }
            return ht;
        }
        public static HT ToHT(XmlNode node)
        {
			_logger.LogDebug("ToHT(XmlNode node) -> " + node.Name);

            HT ht = new HT();
            if (node.Attributes != null) {
                foreach (XmlAttribute attr in node.Attributes) {
                    ht.Add(attr.Name, attr.Value);
                }
            }
            if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.CDATA) {
				_logger.LogDebug("=> text");
				ht.Add("value", node.Value);
            } else if (node.ChildNodes.Count == 1 && (node.ChildNodes[0].NodeType == XmlNodeType.Text || node.ChildNodes[0].NodeType == XmlNodeType.CDATA)) {
				_logger.LogDebug("=> one-child text");
				ht.Add("value", node.ChildNodes[0].Value);
            } else {
				_logger.LogDebug("=> xml");
				foreach (XmlNode child in node.ChildNodes) {
					if (child.ChildNodes.Count == 1 && child.Attributes.Count == 0 && (child.ChildNodes[0].NodeType == XmlNodeType.Text || child.ChildNodes[0].NodeType == XmlNodeType.CDATA)) {
						_logger.LogDebug("_AddChildHT @1 " + child.Name);
                        _AddChildHT(ht, child.Name, NewHT(
							"value", 
							child.ChildNodes[0].Value
						));
                    } else {
						_logger.LogDebug("_AddChildHT @2 " + child.Name);
						_AddChildHT(ht, child.Name, ToHT(child));
                    }
                }
            }

            return ht;
        }
        private static void _AddChildHT(HT ht, object key, object val)
        {
            if (ht.Contains(key)) {
                if (ht[key] is ArrayList) {
                    ((ArrayList)ht[key]).Add(val);
                } else {
                    ArrayList newval = new ArrayList();
                    newval.Add(ht[key]);
                    newval.Add(val);
                    ht[key] = newval;
                }
            } else {
                ht.Add(key, val);
            }
        }

        public static string ToGET(NVC nvc)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string name in nvc.AllKeys) {
                if (nvc[name] == null) continue;
                if (sb.Length > 0) sb.Append("&");
                sb.Append(name);
                sb.Append("=");
                sb.Append(HttpUtility.UrlEncode(nvc[name]));
            }
            return sb.ToString();
        }
        public static string ToGET(HT ht)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object key in ht.Keys) {
                if (ht[key] == null) continue;
                if (sb.Length > 0) sb.Append("&");
                sb.Append(key.ToString());
                sb.Append("=");
                sb.Append(HttpUtility.UrlEncode(ht[key].ToString()));
            }
            return sb.ToString();
        }
        public static string ToAttr(NVC nvc)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string name in nvc.AllKeys) {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(name.ToString());
                sb.Append("=\"");
                sb.Append(nvc[name]);
                sb.Append("\"");
            }
            return sb.ToString();
        }
        public static string ToAttr(HT ht)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object key in ht.Keys) {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(key.ToString());
                sb.Append("=\"");
                sb.Append(ht[key].ToString());
                sb.Append("\"");
            }
            return sb.ToString();
        }

        public static string StripTags(string html)
        {
            Regex regHtml = new Regex("<[^>]*>");
            return regHtml.Replace(html, "");
        }
        public static string PriceFormat(object val)
        {
            return String.Format("{0:#,0}", val);
        }
        public static string PriceFormat(string val)
        {
            return String.Format("{0:#,0}", val);
        }
        public static string PriceFormat(int val)
        {
            return String.Format("{0:#,0}", val);
        }

        public static long ToTicks(DateTime dtInput)
        {
            long ticks = 0;
            ticks = dtInput.Ticks;
            return ticks;
        }
        public static long SubTicks(DateTime date1, DateTime date2)
        {
            return date1.Subtract(date2).Ticks;
        }
        public static DateTime ToDateTime(long lticks)
        {
            DateTime dtresult = new DateTime(lticks);
            return dtresult;
        }

        /* etc
         * ----------------------------------------------------------------------------- */
        public static string RandCode(int length, bool number = true, bool smallAlpha = true, bool bigAlpha = false, bool speicialChar = false)
        {
            StringBuilder sbWord = new StringBuilder();
            StringBuilder sbTicket = new StringBuilder();
            if (number) {
                sbTicket.Append("0123456789");
            }
            if (smallAlpha) {
                sbTicket.Append("abcdefghijklmnopqrstuvwxyz");
            }
            if (bigAlpha) {
                sbTicket.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            }
            if (speicialChar) {
                sbTicket.Append("~!@#$%^&*()_+-=");
            }

            string tickets = sbTicket.ToString();
            Random rand = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < length; i++) {
                int pos = rand.Next(tickets.Length - 1);
                sbWord.Append(tickets.Substring(pos, 1));
            }

            return sbWord.ToString();
        }

        public static string DesEncrypt(string strData, string strKey = "ubitems1")
        {
            byte[] Skey = ASCIIEncoding.ASCII.GetBytes(strKey);
            if (Skey.Length != 8) {
                throw (new Exception("키는 8자이어야 합니다."));

            }  

            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();
            rc2.Key = Skey;
            rc2.IV = Skey;

            MemoryStream ms = new MemoryStream();
            CryptoStream cryStream = new CryptoStream(ms, rc2.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] data = Encoding.UTF8.GetBytes(strData.ToCharArray());
            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string DesDecrypt(string strData,string strKey = "ubitems1")
        {
            byte[] Skey = ASCIIEncoding.ASCII.GetBytes(strKey);
            if (Skey.Length != 8) {
                throw (new Exception("키는 8자이어야 합니다."));

            }
            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();

            rc2.Key = Skey;
            rc2.IV = Skey;

            MemoryStream ms = new MemoryStream();
            CryptoStream cryStream = new CryptoStream(ms, rc2.CreateDecryptor(), CryptoStreamMode.Write);

            byte[] data = Convert.FromBase64String(strData);
            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();

            return Encoding.UTF8.GetString(ms.GetBuffer());
        } 

        /*
         * MD5알고리즘
         * @param type hex or base64
         **/
        public static string MD5(string input, string type = "hex")
        {
            if (input == null) {
                return string.Empty;
            }
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            string retData = "";
            if (type == "base64") {
                retData = Convert.ToBase64String(hashBytes);
            } else if (type == "hex") {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                retData = sb.ToString();
            }

            return retData;
        }

        /*
         * SHA256알고리즘기반 HMAC(해시기반 메세지인증코드)
         * key값로 훼손여부 검증
         * @param type hex or base64
         * @param key 비밀키
         **/
        public static string HMAC_SHA256(string value, string type = "hex", string key = "ubi%&%$&%*")
        {
            if (value == null) {
                return string.Empty;
            }
            byte[] valueByte = System.Text.Encoding.Unicode.GetBytes(value);
            byte[] keyByte = System.Text.Encoding.Unicode.GetBytes(key);

            HMACSHA256 hash = new HMACSHA256(keyByte);
            byte[] hashBytes = hash.ComputeHash(valueByte);

            string retData = "";
            if (type == "base64") {
                retData = Convert.ToBase64String(hashBytes);
            } else if (type == "hex") {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                retData = sb.ToString();
            }

            return retData;
        }

		/*
		 * 가볍게 쓸수 있는 암호화 - 암호화
		 * 표식을 붙여 암호화 여부를 판단한다. 표식이 없을경우 일반 문자열
		 * 표식을 이용하여 일반 문자열과 혼용해서 사용할 수 있다.
		 * deflate/gzip -> base64
		 * ※ 문자열을 첫글자는 표식문자 '*'가 올수 없다.
		 **/
		public static string LiteEncode(string value)
		{
			string encode = null;

			try {
				// to byte
				byte[] byteArray = new byte[value.Length];
				int index = 0;
				foreach (char item in value.ToCharArray()) {
					byteArray[index++] = (byte)item;
				}

				// ready
				MemoryStream ms = new MemoryStream();
				DeflateStream zs = new DeflateStream(ms, CompressionMode.Compress);

				// compress
				zs.Write(byteArray, 0, byteArray.Length);
				zs.Close();

				// to base64
				encode = Convert.ToBase64String(ms.ToArray());

				// 특수문자 교체
				encode = encode.Replace("==", "$");
				//encode = encode.Replace("=", "#");
				//encode = encode.Replace("/", "%");

				ms.Close();
				zs.Dispose();
				ms.Dispose();

				// 표식을 붙여 return
				return "*" + encode;
			} catch (Exception ex) {
				_logger.LogDebug("", ex);
			}
			return value;
		}

		/*
		 * 가볍게 쓸수 있는 암호화 - 복호화
		 * 표식을 붙여 암호화 여부를 판단한다. 표식이 없을경우 일반 문자열
		 * 표식을 이용하여 일반 문자열과 혼용해서 사용할 수 있다.
		 * base64 -> deflate/gzip
		 * ※ 문자열을 첫글자는 표식문자 '*'가 올수 없다.
		 **/
		public static string LiteDecode(string value)
		{
			// 표식 식별
			if (value != null && value.Length > 1 && value.Substring(0, 1) == "*") {
				try {
					string base64 = value.Substring(1);

					// 특수문자 교체
					base64 = base64.Replace("$", "==");
					//base64 = base64.Replace("#", "=");
					//base64 = base64.Replace("%", "/");

					// decode base64
					byte[] byteArray = Convert.FromBase64String(base64);
					int readBytes = 0;

					MemoryStream ms = new MemoryStream(byteArray);
					DeflateStream zs = new DeflateStream(ms, CompressionMode.Decompress);

					// buffer
					byte[] decodeBytes = new byte[512];
					StringBuilder sb = new StringBuilder();

					while ((readBytes = zs.Read(decodeBytes, 0, decodeBytes.Length)) != 0) {
						for (int i = 0; i < readBytes; i++) sb.Append((char)decodeBytes[i]);
					}

					ms.Close();
					zs.Dispose();
					ms.Dispose();

					return sb.ToString();

				} catch (Exception ex) {
					_logger.LogDebug("", ex);
				}
			}

			return value;
		}

        public static string StackTrace()
        {
            StringBuilder sb = new StringBuilder();
            var stackTrace = new StackTrace(true);
            foreach (var r in stackTrace.GetFrames()) {
                Util.SB(sb, string.Format("Filename: {0} Method: {1} Line: {2} Column: {3}  ",
                    r.GetFileName(), r.GetMethod(), r.GetFileLineNumber(),
                    r.GetFileColumnNumber()));
            }
            return sb.ToString();
        }

        public static void StreamCopy(Stream input, Stream output)
        {
            const int bufferSize = 2048;
            byte[] buffer = new byte[bufferSize];
            int read = 0;
            do {
                read = input.Read(buffer, 0, buffer.Length);
                output.Write(buffer, 0, read);
            } while (read >= bufferSize);
        }

        /*
         * 객체내의 value값 반환
         * 훨씬 유연하게 객체에 속한 하위객체의 값을 구할 수 있다.
         * ex) 
         * string memName = "";
         * // 기존
         * if (order != null) {
         *   if (order.Member != null) {
         *     memName = order.Member.Name;
         *   }
         * }
         * // 함수사용
         * memName = Util.V(order, x=>x.Member.Name);
         * 더 복잡한 Util.V(order, x=>x.Product.VendorComp.Name) 등등..
         * 
         * @param model 대상 객체
         * @param express 람다표현식
         **/
        public static TResult V<TModel, TResult>(TModel model, Expression<Func<TModel, TResult>> expression) { return DeepValue<TModel, TResult>(model, expression); }
        public static TResult DeepValue<TModel, TResult>(TModel model, Expression<Func<TModel, TResult>> expression)
        {
            try {
                var func = expression.Compile();
                var value = func(model);
                return value;
            } catch (Exception) {
                return default(TResult);
            }
        }

        public static bool ImageResize(string filePath, string targetPath, int width, int height, bool cut = false, uint argb = 0xFFFFFFFF)
        {
            try {
                int dstHeight = 0;
                int dstWidth = 0;
                using (Image image = Image.Load(filePath)) {
                    if (!cut) {
                        dstWidth = width;
                        dstHeight = (int)Math.Round((float)(width * image.Height) / (float)image.Width);
                        if (dstHeight > height) {
                            dstHeight = height;
                            dstWidth = (int)Math.Round((float)(height * image.Width) / (float)image.Height);
                        }
                    } else {
                        dstWidth = width;
                        dstHeight = (int)Math.Round((float)(width * image.Height) / (float)image.Width);
                        if (dstHeight < height) {
                            dstHeight = height;
                            dstWidth = (int)Math.Round((float)(height * image.Width) / (float)image.Height);
                        }
                    }
                    image.Mutate(x=>x.Resize(width, height));
                    image.Save(targetPath);
                }
            } catch (Exception ex) {
                _logger.LogError("Util:ImageResize", ex);
            }
            return true;
        }

        public static string GetServerIP(int index = 0)
        {
            IList<string> list = new List<string>();
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (var uipi in ni.GetIPProperties().UnicastAddresses) {
                    if (uipi.Address.AddressFamily != AddressFamily.InterNetwork) continue;

                    if (uipi.IPv4Mask == null) continue; //ignore 127.0.0.1
                    list.Add(uipi.Address.ToString());
                }
            }
            return list[index];
        }

        /// <summary>
        /// 우편번호나 전화번호에 '-' 삽입
        /// positions 는 뒷에서부터 자리수 표현
        /// </summary>
        /// <param name="value">삽입할 문자열</param>
        /// <param name="positions">자리수(뒤의 자리부터)</param>
        /// <returns></returns>
        public static string HyphenFormat(string value, int[] positions)
        {
            ArrayList al = new ArrayList();
            int offset = positions.Length - 1;
            int digit = 0;
            string retValue = "";

            foreach (char ch in value.Replace("-", "").ToCharArray().Reverse()) {
                retValue = ch.ToString() + retValue;
                digit++;
                if (offset >= 0 && digit == positions[offset]) {
                    retValue = "-" + retValue;
                    offset--;
                    digit = 0;
                }
            }
            return retValue;
        }
        public static string ConvertUtf8(string str)
        {
            return ConvertEncoding(Encoding.GetEncoding("949"), Encoding.UTF8, str);
        }
        public static string ConvertEuckr(string str)
        {
            return ConvertEncoding(Encoding.UTF8, Encoding.GetEncoding("EUC-KR"), str);
        }
        /*
        public static string decodeEuckrUrl(string url)
        {
            System.Text.Encoding euckr = System.Text.Encoding.GetEncoding(51949);
            byte[] euckrBytes = System.Web.HttpUtility.UrlDecodeToBytes(url);
            return euckr.GetString(euckrBytes);
        }
         * */
        public static string Base64Encode(string strData, System.Text.Encoding encType = null)
        {
            if (encType == null) encType = System.Text.Encoding.ASCII;
            if (string.IsNullOrEmpty(strData)) return "";
            return Convert.ToBase64String(encType.GetBytes(strData));
        }
        public static string Base64Decode(string strData, System.Text.Encoding encType = null)
        {
            if (encType == null) encType = System.Text.Encoding.ASCII;
            if (string.IsNullOrEmpty(strData)) return "";
            byte[] buf = Convert.FromBase64String(strData);
            return encType.GetString(buf);
        }  
        public static string ConvertEncoding(Encoding src, Encoding dst, string str)
        {
            byte[] b = src.GetBytes(str);
            byte[] bConvert = System.Text.Encoding.Convert(src, dst, b);
            string retValue = "";
            foreach (byte bb in bConvert) {
                retValue += ((char)bb).ToString();
            }
            return retValue;
        }

        public static string DecodeHtmlChars(string aText)
        {
            string[] parts = aText.Split(new string[] { "&#" }, StringSplitOptions.None);
            for (int i = 1; i < parts.Length; i++) {
                int n = parts[i].IndexOf(';');
                string number = parts[i].Substring(0, n);
                try {
                    string data = string.Format("{0:X}", int.Parse(number));
                    int unicode = Convert.ToInt32(data, 16);
                    parts[i] = ((char)unicode) + parts[i].Substring(n + 1);
                } catch { }
            }
            return String.Join("", parts);
        }

        public static string MakeData(NVC Form)
        {
            string retValue = "";
            foreach (string key in Form.AllKeys) {
                if (key != "__VIEWSTATE") {
                    string[] array_data = Form[key].Split(',');
                    if (array_data.Length == 1) {
                        if (retValue == "") {
                            retValue = key + "=" + Form[key];
                        }
                        else {
                            retValue += "&" + key + "=" + Form[key];
                        }
                    }
                    else {
                        foreach (string value in array_data) {
                            if (retValue == "") {
                                retValue = key + "=" + value;
                            }
                            else {
                                retValue += "&" + key + "=" + value;
                            }
                        }
                    }
                }
            }
            return retValue;
        }
        public static int GetNotInVat(object Price)
        {
            if (ToInt(Price) == 0) {
                return 0;
            }
            return (int)Math.Round(ToInt(Price) / 1.1, 0);
        }
        public static int GetVat(object Price)
        {
            if (ToInt(Price) == 0) {
                return 0;
            }
            return ToInt(Price) - (int)Math.Round(ToInt(Price) / 1.1, 0);
        }
        public static string LinkString(string targetStr, string str, string separator)
        {
            if (targetStr == "") {
                targetStr = str;
            } else {
                targetStr += separator + str;
            }
            return targetStr;
        }


        public static Dictionary<string, Dictionary<string, object>> DataTableToDictionaty(DataTable dt, string key_column)
        {
            var cols = dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName != key_column);
            return dt.Rows.Cast<DataRow>().ToDictionary(r => r[key_column].ToString(), r => cols.ToDictionary(c => c.ColumnName, c => r[c.ColumnName]));
        }

        public static int[] ToInt32Array(string[] array)
        {
            return array.Select(x => Util.ToInt(x)).ToArray();
        }

        public static string GetIndexString(string[] array, int index, string whitespaceString = "") {
            string result = "";
            if(array.Length > index) {
                result = array[index];
            }
            if(string.IsNullOrWhiteSpace(result)) {
                result = whitespaceString;
            }
            return result;
        }

        public static string GetMaskedValue(string value) {
            return new String('*', Util.S(value).Trim().Length);
        }

        public static string GetMaskedTel(string tel) {
            var value = Util.S(tel).Trim();
            if (value.Length <= 4) return value;

            var pre = value.Substring(0, value.Length - 4);
            var post = value.Substring(value.Length-4, 4);

            var pre1 = "";
            var pre2 = "";
            if(pre.Length <= 4) {
                pre2 = pre;
            } else {
                pre1 = pre.Substring(0, 3);
                pre2 = pre.Substring(3);
            }

            var regex = new Regex("[0-9]", RegexOptions.Compiled);
            pre2 = regex.Replace(pre2, "*");

            return pre1 + pre2 + post;
        }

        public static string GetMaskedEmail(string email) {
            var value = Util.S(email).Trim();

            var atIndex = value.IndexOf('@');
            if(atIndex == -1) return email;

            var pre = value.Substring(0, atIndex);
            var post = value.Substring(atIndex);

            var pre1 = "";
            var pre2 = "";
            if(pre.Length <= 1) {
                pre1 = pre;
            } else {
                int sIdx = pre.Length / 2;
                pre1 = pre.Substring(0, sIdx);
                pre2 = new String('*', pre.Length - sIdx);
            }

            return pre1 + pre2 + post;
        }

        public static string FormCollectionToQueryString(NVC Form)
        {
            string retValue = "";
            foreach (string key in Form.AllKeys) {
                if (key != "__VIEWSTATE") {
                    string[] array_data = Form.GetValues(key);
                    if (array_data.Length == 1) {
                        if (retValue == "") {
                            retValue = key + "=" + Form[key];
                        } else {
                            retValue += "&" + key + "=" + Form[key];
                        }
                    } else {
                        foreach (string value in array_data) {
                            if (retValue == "") {
                                retValue = key + "=" + value;
                            } else {
                                retValue += "&" + key + "=" + value;
                            }
                        }
                    }
                }
            }
            return retValue;
        }

        public static T GetEnumValueAttribute<T>(object agent) where T : class {
            var enumType = agent.GetType();
            var memberInfos = enumType.GetMember(agent.ToString());
            if (memberInfos != null && memberInfos.Length > 0) {
                var memberInfo = memberInfos[0];
                var enumAttrs = memberInfo.GetCustomAttributes(typeof(T), false);
                if (enumAttrs.Length > 0) {
                    var enumAttr = enumAttrs[0] as T;
                    if (enumAttr != null) {
                        return enumAttr;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 태그제거
        /// </summary>
        /// <param name="html">HTML</param>
        /// <returns></returns>
        public static string RemoveTag(string html)
        {
            Regex tag = new Regex("(<([^>]+)>)");
            string removeTag = tag.Replace(html, "");
            return removeTag;
        }

        /// <summary>
        /// 문자열내의 존재하는 단어 찾기
        /// </summary>
        /// <param name="contents">대상 문자열</param>
        /// <param name="words">찾을 문자배열</param>
        /// <returns></returns>
        public static MatchCollection WordsMatch(string contents, string[] words)
        {
            Regex regWords = new Regex("(" + String.Join("|", words) + ")");
            MatchCollection matches = regWords.Matches(contents);
            return matches;
        }
    }
}
