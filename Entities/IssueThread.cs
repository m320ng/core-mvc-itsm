using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using IssueTracker.Entities.Interfaces;
using IssueTracker.Entities.Components;

namespace IssueTracker.Entities
{
    public enum IssueThreadType {
        일반 = 0,
        접수,
        완료,
        검수확인,
        재수정,
        반려 = 9,
        추가수정접수 = 101,
        추가수정완료 = 102,
    }

    public class IssueThread : IAuditable
    {
        [Display(Name = "고유번호")]
        public virtual int Id { get; set; }

        [Display(Name = "종류")]
        public virtual IssueThreadType Type { get; set; }

        // 접수,완료,검수확인
        [Display(Name = "이전상태")]
        public virtual IssueState? OldState { get; set; }
        [Display(Name = "상태")]
        public virtual IssueState? State { get; set; }

        // 접수
        [Display(Name = "예정일자"), DataType(DataType.Date)]
        public virtual DateTime? CompleteDueDate { get; set; }

        [Display(Name = "제목"), Required]
        public virtual string Subject { get; set; }
        [Display(Name = "내용"), Required]
        public virtual string Content { get; set; }

        [Display(Name = "직원")]
        public virtual IssueEmployee IssueEmployee { get; set; }
        [Display(Name = "개발자여부")]
        public virtual bool IsDeveloper { get; set; }
        [Display(Name = "이슈")]
        public virtual Issue Issue { get; set; }
        [Display(Name = "이메일발송여부")]
        public virtual bool IsSendMail { get; set; }

        public virtual bool IsDelete { get; set; }
        [Display(Name = "생성유저")]
        public User CreateBy { get; set; }
        [Display(Name = "수정유저")]
        public User UpdateBy { get; set; }
        [Display(Name = "생성일자")]
        public DateTime Created { get; set; }
        [Display(Name = "수정일자")]
        public DateTime? Updated { get; set; }

        public IssueThread()
        {
        }

    }
}
