using System;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using NHibernate;
using NHibernate.Linq;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class SupportController : Controller
    {
        private readonly ISession _session;

        public SupportController(ISession session)
        {
            _session = session;
        }

        public JsonNetResult Index()
        {
            var mpsWithError =
                (from mp in _session.Query<MP_CustomerMarketPlace>()
                where mp.UpdateError != null && mp.UpdateError != ""
                select new SupportModel
                {
                    Umi=mp.Id,
                    Type = mp.Marketplace.Name,
                    CustomerId = mp.Customer.Id,
                    ErrorMessage = mp.UpdateError,
                    Name = mp.DisplayName,
                    UpdateStartDate = FormattingUtils.FormatDateTimeToString(mp.UpdatingStart,"")
                }).ToList();
            return this.JsonNet(mpsWithError);
        }

    }
}
