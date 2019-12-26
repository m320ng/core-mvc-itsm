

using System.Collections.Generic;

namespace IssueTracker.Models {


    public class PaginationRequest {
        public int page {get; set;}
        public int limit {get; set;}
        public Sort sort {get; set;}
        public Condition[] conditions {get; set;}

        public class Sort {
            public string field {get; set;}
            public string dir {get; set;}
        }
        public class Condition {
            public string field {get; set;}
            public string op {get; set;}
            public string value {get; set;}
        }
    }

}