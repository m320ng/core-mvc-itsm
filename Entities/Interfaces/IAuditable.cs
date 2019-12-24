using System;
using IssueTracker.Entities;

namespace IssueTracker.Entities.Interfaces
{
    public interface IAuditable
    {
        public User CreateBy { get; set; }
        public User UpdateBy { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
