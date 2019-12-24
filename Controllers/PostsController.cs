using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IssueTracker.Services;
using IssueTracker.Models;
using IssueTracker.Entities;

namespace IssueTracker.Controllers {
    public class SignInModel {
        public string email { get; set; }
        public string password { get; set; }
    }

    [Authorize]
    [ApiController]
    [Route("/v1/")]
    public class PostsController : ControllerBase {
        private IssueEmployeeService _issueEmpService;

        public PostsController(IssueEmployeeService issueEmpService) {
            _issueEmpService = issueEmpService;
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public IActionResult SignIn([FromBody]SignInModel model) {
            var emp = _issueEmpService.Login(model.email, model.password, null);

            if (emp == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var user = emp.User;
            user.Permissions = new string[] { "Admin" };

            return Ok(emp.User);
        }

        [AllowAnonymous]
        [HttpGet("posts")]
        public IActionResult GetAll([FromQuery]int limit) {
            var list = _issueEmpService.Pagination(limit);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id) {
            // only allow admins to access other user records
            var currentUserId = int.Parse(User.Identity.Name);
            if (id != currentUserId && !User.IsInRole(Role.Admin))
                return Forbid();

            var user = _issueEmpService.Get(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}