using System.Collections.Generic;
using System.Linq;

namespace IssueTracker.Models {
    public class PaginationList<T> {
        public int page { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
        public int pages { get; set; }
        public ICollection<T> data { get; set; }

        public static PaginationList<T> Pagination(IQueryable<T> list, int page, int limit) {
            var pagedList = new PaginationList<T>();
            var skip = (page - 1) * limit;
            pagedList.data = list.Take(page).Skip(skip).ToList();
            pagedList.page = page;
            pagedList.limit = limit;
            return pagedList;
        }
    }
}
