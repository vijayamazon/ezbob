﻿using System;
using System.Web.Mvc;
using EZBob.DatabaseLib;
using EzBob.CommonLib;
using EzBob.Web.Code;
using EzBob.Web.Code.Agreements;
using EzBob.Web.Infrastructure;
using StructureMap;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class AgreementController : Controller
    {
        private readonly AgreementRenderer _agreementRenderer;
        private readonly IEzbobWorkplaceContext _context;
        private readonly AgreementsModelBuilder _builder;
        private readonly AgreementsTemplatesProvider _templates;
        private readonly LoanBuilder _loanBuilder;
        private EZBob.DatabaseLib.Model.Database.Customer _customer;

        public AgreementController(AgreementRenderer agreementRenderer, IEzbobWorkplaceContext context, AgreementsModelBuilder builder, AgreementsTemplatesProvider templates, LoanBuilder loanBuilder)
        {
            _agreementRenderer = agreementRenderer;
            _context = context;
            _builder = builder;
            _templates = templates;
            _customer = _context.Customer;
            _loanBuilder = loanBuilder;
        }

        public FileResult Download(decimal amount, string viewName, int loanType, int repaymentPeriod)
        {
            var lastCashRequest = _customer.LastCashRequest;

			if (_customer.IsLoanTypeSelectionAllowed == 1) {
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
				lastCashRequest.RepaymentPeriod = repaymentPeriod;
				lastCashRequest.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
			} // if

            var loan = _loanBuilder.CreateLoan(lastCashRequest, amount, DateTime.UtcNow);
            
            string file = _templates.GetTemplateByName(viewName);

            var model = _builder.Build(_customer, amount, loan);
            var pdf = _agreementRenderer.RenderAgreementToPdf(file, model);
            return File(pdf, "application/pdf", viewName + " Summary_" + DateTime.Now + ".pdf");
        }
    }
}
