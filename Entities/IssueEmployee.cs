using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using IssueTracker.Entities.Interfaces;
using IssueTracker.Entities.Components;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Entities
{
    public enum IssueEmployeeManageType {
        자체관리 = 0,
        연동관리
    }

    public class IssueEmployee : IAuditable
    {
        [ScaffoldColumn(false)]
        [Display(Name = "고유번호")]
        public int Id { get; set; }
        [Display(Name = "연동구분"), UIHint("EnumValue")]
        public IssueEmployeeManageType ManageType { get; set; }
        [Display(Name = "이름"), Required]
        public string Name { get; set; }
        [Display(Name = "접속아이디"), Required]
        public string Account { get; set; }
        [Display(Name = "비밀번호"), Required, DataType(DataType.Password)]
        public string Password { get; set; }
        [NotMapped]
        [Display(Name = "비밀번호 확인"), Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "비밀번호와 같지 않습니다.")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "이메일"), DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Display(Name = "전화"), DataType(DataType.PhoneNumber), UIHint("Tel")]
        public string Tel { get; set; }
        [Display(Name = "핸드폰"), DataType(DataType.PhoneNumber), UIHint("Phone")]
        public string Phone { get; set; }
        [Display(Name = "사번"), Required]
        public string EmployeeNo { get; set; }
        [Display(Name = "비고")]
        public string Remark { get; set; }
        [Display(Name = "팀코드")]
        public string TeamCode { get; set; }
        [Display(Name = "팀명")]
        public string TeamName { get; set; }
        [Display(Name = "새댓글수")]
        public int NewReplyThread { get; set; }
        [Display(Name = "개발자여부")]
        public bool IsDeveloper { get; set; }
        [Display(Name = "관리자여부")]
        public bool IsAdmin { get; set; }

        [Display(Name = "그룹코드", Description = "별도로 그룹화하기 위한 코드")]
        public string GroupCode { get; set; }

        [Column("User_id")]
        public User User { get; set; }

        public bool IsDelete { get; set; }
        [Display(Name = "생성유저")]
        [Column("CreateBy")]
        public User CreateBy { get; set; }
        [Display(Name = "수정유저")]
        [Column("UpdateBy")]
        public User UpdateBy { get; set; }
        [Display(Name = "생성일자")]
        public DateTime Created { get; set; }
        [Display(Name = "수정일자")]
        public DateTime? Updated { get; set; }

        public IssueEmployee()
        {
        }
    }
}
