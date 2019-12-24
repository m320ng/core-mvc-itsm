using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Helpers
{
    public class EntityHelper
    {
        public static ILogger _logger;

        public EntityHelper()
        {
        }

        /*
         * target 오브젝트를 source 내용으로 Merge
         * @param target
         * @param source
         * @param exclude 제외할 필드
         * @param include 포함할 필드
         * @param nullMerge NULL Merge 여부
         * @param zeroListMerge List 0개 Item Merge 여부
         * */
        public static void Merge(object target, object source, string exclude = null, string include = null, bool nullMerge = false, bool zeroListMerge = false)
        {
            if (target.GetType() != source.GetType()) {
                new Exception("객체의 타입이 서로 다릅니다. (" + target.GetType() + "!=" + source.GetType()+")");
            }
            Type type = target.GetType();
            foreach (var prop in type.GetProperties()) {
                if (!prop.CanWrite) continue;
                var val = prop.GetValue(source, null);
                if (!nullMerge) {
                    if (val == null) continue;
                }
                if (!zeroListMerge) {
                    IList list = val as IList;
                    if (list != null) {
                        if (list.Count==0) continue;
                    }
                }
                if (!string.IsNullOrEmpty(exclude)) {
                    var excludeArray = exclude.Split(',');
                    if (excludeArray.Contains(prop.Name)) continue;
                }
                if (!string.IsNullOrEmpty(include)) {
                    var includeArray = include.Split(',');
                    if (!includeArray.Contains(prop.Name)) continue;
                    prop.SetValue(target, val, null);
                } else {
                    try {
                        prop.SetValue(target, val, null);
                    } catch (Exception ex) {
                        _logger.LogError("prop:" + prop.Name + ",val:" + val, ex);
                    }
                }
            }
        }

        /*
         * target 오브젝트를 source 내용으로 Merge (Value만)
         * @param target
         * @param source
         * @param exclude 제외할 필드
         * @pagam include 포함할 필드
         * */
        public static void MergeValue(object target, object source, string exclude = null, string include = null)
        {
            if (target.GetType() != source.GetType()) {
                new Exception("객체의 타입이 서로 다릅니다.");
            }
            Type type = target.GetType();
            foreach (var prop in type.GetProperties()) {
                var val = prop.GetValue(source, null);
                if (val == null) continue;
                if (!prop.PropertyType.IsValueType && prop.PropertyType!=typeof(string)) continue;
                if (!string.IsNullOrEmpty(exclude)) {
                    var excludeArray = exclude.Split(',');
                    if (excludeArray.Contains(prop.Name)) continue;
                }
                if (!string.IsNullOrEmpty(include)) {
                    var includeArray = include.Split(',');
                    if (!includeArray.Contains(prop.Name)) continue;
                    prop.SetValue(target, val, null);
                } else {
                    try {
                        prop.SetValue(target, val, null);
                    } catch (Exception ex) {
                        _logger.LogError("prop:" + prop.Name + ",val:" + val, ex);
                    }
                }
            }
        }

        public DataTable ToDataTable(IList list, string dataTableName = "")
        {
            DataTable dt = new DataTable(dataTableName);
            foreach (var prop in list[0].GetType().GetProperties()) {
                dt.Columns.Add(prop.Name);
            }
            foreach (object obj in list) {
                object[] fieldList = obj as object[];
                dt.Rows.Add(fieldList);
            }
            return dt;
        }
    }
}
