using System;

namespace IT_WorkPlant.Models
{
    public class WF_EmailModel
    {
        public int FormID { get; set; }
        public string FormType { get; set; }
        public string RequesterName { get; set; }
        public string RequesterDept { get; set; }
        public DateTime RequestDate { get; set; }
        public string ApproverName { get; set; }
        public string ApproverEmail { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public string ApprovalLink { get; set; }
        public string FormContent { get; set; }
        public string Token { get; set; }
    }
} 