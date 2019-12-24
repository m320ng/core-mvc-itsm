using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Linq.Expressions;
using IssueTracker.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Entities.Components
{
    public class UploadFile
    {
        [Display(Name = "파일명")]
        public virtual string Name { get; set; }
        [Display(Name = "경로")]
        public virtual string Path { get; set; }
        [Display(Name = "URL")]
        public virtual string Url { get; set; }
        
        /* local */
        [NotMapped]
        public virtual string OldPath { get; set; }
        [NotMapped]
        public virtual string OldUrl { get; set; }
        [NotMapped]
        public virtual string TempPath { get; set; }
        [NotMapped]
        public virtual string TempUrl { get; set; }
        [NotMapped]
        public virtual bool? IsUpload { get; set; }

        public UploadFile()
        {
            IsUpload = false;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Path);
        }

        public long GetSize()
        {
            long ret = -1;
            try {
                ret = new FileInfo(Path).Length;
            } catch (Exception) { }
            return ret;
        }
        public static void SaveManaged<T>(T entity, Expression<Func<T, UploadFile>> expression, string id = null, bool deleteTempFile = true)
        {
            var func = expression.Compile();
            UploadFile file = func.Invoke(entity);
            if (file != null) {
                MemberExpression memberExpression = null;
                if (expression.Body.NodeType == ExpressionType.Convert) {
                    memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
                } else if (expression.Body.NodeType == ExpressionType.MemberAccess) {
                    memberExpression = expression.Body as MemberExpression;
                }
                file.SaveManaged(entity, memberExpression.Member.Name, id, deleteTempFile);
            }
        }
        public static void SaveManaged(UploadFile file, object entity, string columnName, string id = null, bool deleteTempFile = true)
        {
            if (file != null) file.SaveManaged(entity, columnName, id, deleteTempFile);
        }

        public virtual void SaveManaged(object entity, string columnName, string id = null, bool deleteTempFile = true)
        {
            if (IsUpload == true && IsValid()) {
                if (id == null) id = entity.GetPropValue("Id").ToString();

                int group = (int)Math.Ceiling((float)Util.ToInt(id) / (float)10000) * 10000;

                string entityType = entity.GetType().Name;
                if (Util.Substring(entityType, -5) == "Proxy") {
                    entityType = Util.Substring(entityType, 0, entityType.Length - 5);
                }

                var path = GetPath(entityType, "" + group) + "/" + id;
                var file = path + "/" + columnName;
                string ext = global::System.IO.Path.GetExtension(Name);
                if (!string.IsNullOrEmpty(ext)) file = file + ext.ToLower();
                //StorageManager.MakeStoragePath(path);
                //Directory.CreateDirectory(path);

                TempPath = Path;
                TempUrl = Url;
                //Url = StorageManager.Url(file);
                Path = file;

                if (!string.IsNullOrEmpty(OldPath)) {
                    //StorageManager.Delete(OldPath);
                }

                //_logger.LogDebug("MoveLocalToStorage:" + TempPath + ", " + Path);
                if (deleteTempFile) {
                    //StorageManager.MoveLocalToStorage(TempPath, Path);
                } else {
                    //StorageManager.CopyLocalToStorage(TempPath, Path);
                }
            }
        }

        public void Rollback()
        {
            if (IsUpload == true) {
                if (!string.IsNullOrEmpty(Path) || !string.IsNullOrEmpty(TempPath)) return;
                //StorageManager.MoveStorageToLocal(Path, TempPath);
                Url = TempUrl;
                Path = TempPath;
            }
        }

        public string GetPath(string entity, string group = null)
        {
            if (group == null) group = DateTime.Now.ToString("yyyy-MM-dd");
            return "/" + entity + "/" + group;
        }

        public override bool Equals(object obj)
        {
            var target = obj as UploadFile;
            if (target == null) return false;
            return this.Name == target.Name
                && this.Path == target.Path
                && this.Url == target.Url;
        }
        public override string ToString()
        {
            return this.Name + " " + this.Path + " " + this.Url;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
