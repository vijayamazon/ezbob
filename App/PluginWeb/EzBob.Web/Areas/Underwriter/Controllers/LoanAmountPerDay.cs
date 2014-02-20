﻿namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using Dapper;
	using Infrastructure;
	using NHibernate;

    public class LoanAmountPerDayController : Controller
    {
        private readonly ISession _session;

        public LoanAmountPerDayController(ISession session)
        {
            _session = session;
        }

        public ViewResult Index ()
        {
            var items = _session.Connection.Query("select * from vw_LoansAmountByDay where Date > @date", new{date = new DateTime(2012, 9, 01)}, _session.GetTransaction())
                        .Select(i => new[] { i.Date.ToString(CultureInfo.InvariantCulture), i.Amount });
            return View(items);
        }
    }
}