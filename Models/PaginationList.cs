using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace IssueTracker.Models {
    public class PaginationList<T> {
        public int page { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
        public int pages { get; set; }
        public ICollection<T> data { get; set; }

        public static PaginationList Pagination(IQueryable<T> list, int page, int limit) {
            var pagedList = new PaginationList();
            var skip = (page - 1) * limit;
            list = list.Take(page).Skip(skip);
            pagedList.total = list.Count();
            pagedList.data = list.ToList();
            pagedList.page = page;
            pagedList.pages = (int)Math.Ceiling((float)pagedList.total / (float)limit);
            pagedList.limit = limit;
            return pagedList;
        }
    }

    public class PaginationList {
        public int page { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
        public int pages { get; set; }
        public ICollection data { get; set; }

        public static PaginationList Pagination(IQueryable<object> list, int page, int limit) {
            var pagedList = new PaginationList();
            var skip = (page - 1) * limit;
            list = list.Take(page).Skip(skip);
            pagedList.total = list.Count();
            pagedList.data = list.ToList();
            pagedList.page = page;
            pagedList.pages = (int)Math.Ceiling((float)pagedList.total / (float)limit);
            pagedList.limit = limit;
            return pagedList;
        }

        public static PaginationList Pagination<T>(IQueryable<T> list, int page, int limit) {
            var pagedList = new PaginationList();
            var skip = (page - 1) * limit;
            list = list.Take(page).Skip(skip);
            pagedList.total = list.Count();
            pagedList.data = list.ToList();
            pagedList.page = page;
            pagedList.pages = (int)Math.Ceiling((float)pagedList.total / (float)limit);
            pagedList.limit = limit;
            return pagedList;
        }
    }
}
