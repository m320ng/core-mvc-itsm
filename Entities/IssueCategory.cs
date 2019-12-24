using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using IssueTracker.Entities.Components;
using IssueTracker.Entities.Interfaces;

namespace IssueTracker.Entities
{
    public class IssueCategory : IAuditable
    {
        [ScaffoldColumn(false)]
        [Display(Name = "고유번호")]
        public virtual int Id { get; set; }
        [Display(Name = "코드")]
        public virtual string Code { get; set; }
        [Display(Name = "이름")]
        public virtual string Name { get; set; }
        public virtual IssueCategory Parent { get; set; }
        [Display(Name = "순서")]
        public virtual int Position { get; set; }
        public virtual int Depth { get; set; }
        public virtual IList<IssueCategory> Childs { get; set; }
        [Display(Name = "전체순서")]
        public virtual string Ordered { get; set; }
        [Display(Name = "경로")]
        public virtual string Path { get; set; }
        [Display(Name = "최하위노드")]
        public virtual bool IsLeafNode { get; set; }
        [Display(Name = "활성여부")]
        public virtual bool IsActive { get; set; }

        public virtual bool IsDelete { get; set; }
        [Display(Name = "생성유저")]
        public User CreateBy { get; set; }
        [Display(Name = "수정유저")]
        public User UpdateBy { get; set; }
        [Display(Name = "생성일자")]
        public DateTime Created { get; set; }
        [Display(Name = "수정일자")]
        public DateTime? Updated { get; set; }

        [Display(Name = "대분류")]
        public virtual IssueCategory IssueCategory1 { get; set; }
        [Display(Name = "중분류")]
        public virtual IssueCategory IssueCategory2 { get; set; }
        [Display(Name = "소분류")]
        public virtual IssueCategory IssueCategory3 { get; set; }
        [Display(Name = "세분류")]
        public virtual IssueCategory IssueCategory4 { get; set; }

        [Display(Name = "대분류명")]
        public virtual string IssueCategoryName1 { get; set; }
        [Display(Name = "중분류명")]
        public virtual string IssueCategoryName2 { get; set; }
        [Display(Name = "소분류명")]
        public virtual string IssueCategoryName3 { get; set; }
        [Display(Name = "세분류명")]
        public virtual string IssueCategoryName4 { get; set; }

        [Display(Name = "개발승인자")]
        public virtual string AcceptAllowIssueEmployee { get; set; }

        public IssueCategory()
        {
            Depth = 1;
            Position = 0;
            Childs = new List<IssueCategory>();
        }
    }
}
