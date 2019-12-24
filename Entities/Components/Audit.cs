using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IssueTracker.Models;

namespace IssueTracker.Entities.Components
{
    public class Audit 
    {
        [Display(Name = "생성유저")]
        [ForeignKey("CreateUserId")]
        public User CreateUser { get; set; }
        [Display(Name = "수정유저")]
        [ForeignKey("UpdateUserId")]
        public User UpdateUser { get; set; }
        [Display(Name = "생성일자")]
        [Column("Created")]
        public DateTime Created { get; set; }
        [Display(Name = "수정일자")]
        [Column("Updated")]
        public DateTime? Updated { get; set; }

        public Audit()
        {
        }
    }
}
