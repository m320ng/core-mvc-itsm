using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using IssueTracker.Entities.Components;
using IssueTracker.Entities.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Entities
{
    public enum IssueType {
        전자결재문서,
        단순요청 = 0
    }

    public enum IssueDocumentType {
        시스템개발수정요청서,
        [Display(Name = "(온라인)시스템개발수정요청서")]
        온라인시스템개발수정요청서,
        전산자료요청서,
        전표수정요청서
    }

    public enum IssueDraftState {
        기안,
        결제,
        반려
    }

    public enum IssueRequestType {
        개발 = 0,
        디자인,
    }

    public enum IssueAcceptType {
        신규개발 = 0,
        프로세스변경,
        단순수정,
        자료요청,
        데이터수정요청,
        디자인요청
    }

    public enum IssueUploadFileType {
        [Display(Name = "시스템 개발/수정요청서")]
        시스템개발수정요청서 = 0,
        전표수정요청서,
        자료요청서,
        회원자료요청서
    }

    public enum IssueState {
        요청 = 0,
        진행중,
        완료,
        반려 = 9,
    }

    public enum IssueAfterServiceState {
        없음 = 0,
        추가수정접수 = 101,
        추가수정완료 = 102,
    }

    public enum IssueConfirmState {
        검수대기 = 0,
        검수중 = 1,
        검수완료 = 2,
        재수정 = 99,
    }

    public class Issue : IAuditable
    {
        [Display(Name = "고유번호")]
        public virtual int Id { get; set; }

        [Display(Name = "구분")]
        public virtual IssueType Type { get; set; }

        [Display(Name = "기안상태")]
        public virtual IssueDraftState? DraftState { get; set; }

        [Display(Name = "시스템"), Required]
        public virtual IssueCategory Category { get; set; }

        [Display(Name = "문서구분")]
        public virtual IssueDocumentType? DocumentType { get; set; }
        [Display(Name = "요청구분"), Required]
        public virtual IssueRequestType RequestType { get; set; }
        [Display(Name = "접수구분"), Required]
        public virtual IssueAcceptType? AcceptType { get; set; }
        [Display(Name = "요청서상태")]
        public virtual IssueState State { get; set; }
        [Display(Name = "검수상태")]
        public virtual IssueConfirmState ConfirmState { get; set; }

        [Display(Name = "제목"), Required]
        public virtual string Subject { get; set; }
        [Display(Name = "내용"), Required]
        public virtual string Content { get; set; }
        [Display(Name = "쓰레드수")]
        public virtual int ThreadCount { get; set; }
        [Display(Name = "요청기한"), DataType(DataType.Date)]
        public virtual DateTime? RequestCompleteDate { get; set; }
        [Display(Name = "요청기한(조정)"), DataType(DataType.Date)]
        public virtual DateTime? RequestDisputeCompleteDate { get; set; }
        [Display(Name = "요청일자"), DataType(DataType.Date), Required]
        public virtual DateTime? RequestDate { get; set; }
        [Display(Name = "기안일자"), DataType(DataType.Date)]
        public virtual DateTime? DraftDate { get; set; }
        [Display(Name = "접수일자")]
        public virtual DateTime? AcceptDate { get; set; }
        [Display(Name = "완료일자")]
        public virtual DateTime? CompleteDate { get; set; }
        [Display(Name = "완료예정일자"), DataType(DataType.Date)]
        public virtual DateTime? CompleteDueDate { get; set; }
        [Display(Name = "개발승인일자"), DataType(DataType.Date), Required]
        public virtual DateTime? AcceptAllowDate { get; set; }
        [Display(Name = "검수완료일자")]
        public virtual DateTime? ConfirmDate { get; set; }
        [Display(Name = "이관일자"), DataType(DataType.Date)]
        public virtual DateTime? TransferDate { get; set; }
        
        [Display(Name = "이관여부")]
        public virtual bool IsTransfer { get; set; }

        [Display(Name = "요청자 연락처")]
        public virtual string Tel { get; set; }
        [Display(Name = "요청자 이메일")]
        public virtual string Email { get; set; }

        [Display(Name = "요청서 문서번호")]
        public virtual string DocumentNo { get; set; }

        [Display(Name = "인터페이스ID")]
        public virtual int? InterfaceID { get; set; }
        [Display(Name = "인터페이스외부링크")]
        public virtual string InterfaceDocUrl { get; set; }
        [Display(Name = "인터페이스Updated")]
        public virtual DateTime? InterfaceUpdated { get; set; }

        //[Display(Name = "요청서")]
        public virtual UploadFile UploadFile { get; set; }
        [Display(Name = "요청서구분")]
        public virtual IssueUploadFileType? UploadFileType { get; set; }

        [Display(Name = "작성자")]
        public virtual IssueEmployee CreateIssueEmployee { get; set; }
        [Display(Name = "요청자")]
        public virtual IssueEmployee RequestIssueEmployee { get; set; }
        [Display(Name = "접수개발자")]
        public virtual IssueEmployee AcceptIssueEmployee { get; set; }
        [Display(Name = "완료개발자")]
        public virtual IssueEmployee CompleteIssueEmployee { get; set; }
        [Display(Name = "검수완료직원")]
        public virtual IssueEmployee ConfirmIssueEmployee { get; set; }

        [Display(Name = "이관자")]
        public virtual string TransferIssueEmployee { get; set; }
        [Display(Name = "개발승인자")]
        public virtual string AcceptAllowIssueEmployee { get; set; }

        [Display(Name = "접수쓰래드")]
        public virtual IssueThread AcceptIssueThread { get; set; }
        [Display(Name = "완료쓰래드")]
        public virtual IssueThread CompleteIssueThread { get; set; }
        [Display(Name = "쓰래드")]
        [InverseProperty("Issue")]
        public virtual IList<IssueThread> ThreadList { get; set; }

        [Display(Name = "추가수정상태")]
        public virtual IssueAfterServiceState AfterServiceState { get; set; }
        [Display(Name = "추가수정접수일")]
        public virtual DateTime? AfterServiceAcceptDate { get; set; }
        [Display(Name = "추가수정완료예정일"), DataType(DataType.Date)]
        public virtual DateTime? AfterServiceCompleteDueDate { get; set; }
        [Display(Name = "추가수정완료일"), DataType(DataType.Date)]
        public virtual DateTime? AfterServiceCompleteDate { get; set; }

        public virtual bool IsDelete { get; set; }
        [Display(Name = "생성유저")]
        public User CreateBy { get; set; }
        [Display(Name = "수정유저")]
        public User UpdateBy { get; set; }
        [Display(Name = "생성일자")]
        public DateTime Created { get; set; }
        [Display(Name = "수정일자")]
        public DateTime? Updated { get; set; }

        public Issue()
        {
            ThreadList = new List<IssueThread>();
        }

    }
}
