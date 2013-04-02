using System;
using System.Collections.Generic;
using Scorto.NHibernate.Model;

namespace EzBob.Web.Areas.Customer.Models
{
    public class AccountSettingsModel
    {
        public SecurityQuestionModel SecurityQuestionModel { get; set; }

        public IList<SecurityQuestion> SecurityQuestions { get; set; }
    }
}