﻿using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;
using PaymentServices.Calculators;

namespace EzBob.Web.Code
{
    public class LoanHistoryExelReportResult:ActionResult
    {
        private readonly Customer _customer;

        public LoanHistoryExelReportResult(Customer customer)
        {
            _customer = customer;
        }

        public override void ExecuteResult(ControllerContext context)
        { 
            var generator = new LoanHistoryExelReportGenerator();
            var loans = _customer.Loans.Select(l => LoanModel.FromLoan(l, new PayEarlyCalculator2(l, null)) ).ToList();
            var content = generator.GenerateReport( loans, _customer.PersonalInfo.Fullname);
            var f = new FileContentResult(content, "application/xls") { FileDownloadName = "LoanHistory(" + _customer.PersonalInfo.Fullname + ").xls" };
            f.ExecuteResult(context);
        }
    }
}