using System;

namespace EzBob.Web.Areas.Customer.Models
{
    public class SecurityQuestionModel
    {
        public int Question { get; set; }
        public string Answer { get; set; }

        //---------------------------------------------
        public SecurityQuestionModel()
        {
            Question = 0;
            Answer = "";
        }
    }
}
