using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Identity;

namespace IssueTracker.Entities {
    public enum UserType {
        운영자 = 1,
        협력사,
        고객사,
        고객
    }
    
    public static class Role
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public class User {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "이름")]
        public string Name { get; set; }
        [Display(Name = "아이디")]
        public string Username { get; set; }
        [Display(Name = "비밀번호")]
        public string Password { get; set; }
        [Display(Name = "롤")]
        public string Role { get; set; }
        [NotMapped]
        public string[] Permissions {get; set;}
        [Display(Name = "토큰")]
        public string Token { get; set; }

        [Display(Name = "회원종류")]
        public UserType Type { get; set; }
        public bool IsDelete { get; set; }

        /* local */
        [NotMapped]
        public int UserSessionId { get; set; }

        public User() { }
    }
}