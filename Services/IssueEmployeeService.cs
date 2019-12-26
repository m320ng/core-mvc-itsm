using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

using IssueTracker.Data;
using IssueTracker.Helpers;
using IssueTracker.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IssueTracker.Models;

namespace IssueTracker.Services {
    public class IssueEmployeeService {
        private readonly ILogger<IssueEmployeeService> _logger;
        private readonly IssueTrackerContext _context;

        private readonly AppSettings _appSettings;

        public IssueEmployeeService(ILogger
            <IssueEmployeeService> logger,
            IssueTrackerContext context,
            IOptions<AppSettings> appSettings) {
            _logger = logger;
            _context = context;
            _appSettings = appSettings.Value;
        }

        public void Delete(object id) {
            try {
                IssueEmployee data = _context.IssueEmployee.Find(id);
                data.IsDelete = true;
                _context.SaveChanges();
            } catch (Exception ex) {
                _logger.LogError("", ex);
                throw ex;
            }
        }

        public void Delete(int[] list) {
            var query = from x in _context.IssueEmployee
                        where list.Contains(x.Id)
                        select x;
            foreach (var item in query) {
                item.IsDelete = true;
            }
            _context.SaveChanges();
        }

        public IssueEmployee Login(string account, string password, string ip) {
            IssueEmployee emp = null;
            try {
                emp = (from c in _context.IssueEmployee.Include(x=>x.User)
                       where c.IsDelete == false && c.Account == account
                       //     && c.SystemType == systemType
                       select c).FirstOrDefault();
                if (emp != null && emp.Password == password) {
                    if (emp.User == null) {
                        emp.User = new User() {
                            Name = emp.Name,
                            Type = UserType.운영자
                        };
                    }
                    _logger.LogInformation("_appSettings.Secret:"+_appSettings.Secret);

                    // authentication successful so generate jwt token
                    var user = emp.User;
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                    var tokenDescriptor = new SecurityTokenDescriptor {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, user.Id.ToString()),
                            new Claim(ClaimTypes.Role, "Admin"),
                            new Claim("permissions", "['admin']")
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    emp.User.Token = tokenHandler.WriteToken(token);

                    _context.SaveChanges();

                } else {
                    return null;
                }
            } catch (Exception ex) {
                _logger.LogError("", ex);
                throw ex;
            }
            
            _context.Entry(emp).State = EntityState.Deleted;
            return emp;
        }

        public IssueEmployee Get(int? id, bool deleteCheck = false) {
            var item = _context.IssueEmployee.AsNoTracking()
                            .Where(x=>x.Id==id).FirstOrDefault();
            if (deleteCheck && item.IsDelete == true) return null;
            return item;
        }

        public IssueEmployee GetByAccount(string account) {
            IssueEmployee emp = null;
            emp = (from e in _context.IssueEmployee.AsNoTracking()
                   where e.IsDelete == false && e.Account == account
                   select e).FirstOrDefault();
            return emp;
        }

        public void Save(IssueEmployee data) {
            var persist = _context.IssueEmployee
                            .Where(x=>x.Id==data.Id).FirstOrDefault();
            if (persist == null) {
                if (data.User == null) {
                    data.User = new User() {
                        Name = data.Name,
                        Type = UserType.운영자
                    };
                }
                data.User.Name = data.Name;
                _context.IssueEmployee.Add(data);
                _context.SaveChanges();
            } else {
                EntityHelper.Merge(persist, data);

                if (persist.User == null) {
                    persist.User = new User() {
                        Name = persist.Name,
                        Type = UserType.운영자
                    };

                }
                _context.SaveChanges();
            }
        }

        public IQueryable<IssueEmployee> All() {
            var query = _context.IssueEmployee.AsNoTracking();
            return query;
        }

        public IList<IssueEmployee> GetAll() {
            return _context.IssueEmployee.AsNoTracking().ToList();
        }
    }
}