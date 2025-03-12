using System;
using System.Collections.Generic;

namespace IT_WorkPlant.Models
{
    public class EmailRequestSubmissionModel
    {
        public DateTime RequestDate { get; set; }
        public string RequesterName { get; set; }
        public string DeptName { get; set; }
        public List<EmailRequestModel> Requests { get; set; } = new List<EmailRequestModel>();
    }
}
