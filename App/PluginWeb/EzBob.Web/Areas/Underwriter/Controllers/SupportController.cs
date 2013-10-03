using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Underwriter.Models;
using NHibernate;
using NHibernate.Linq;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class SupportController : Controller
    {
        public enum SortField
        {
            Umi = 1,
            MpType,
            MpName,
            UpdateStartDate,
            ErrorMessage,
            CustomerId,
            Status
        }

        private readonly ISession _session;

        public SupportController(ISession session)
        {
            _session = session;
        }

        public JsonNetResult Index(SortField sortField, string sortType)
        {
            List<SupportModel> models =
                (from mp in _session.Query<MP_CustomerMarketPlace>()
                    where mp.UpdateError != null && mp.UpdateError != ""
                    select new SupportModel
                           {
                               Umi = mp.Id,
                               Type = mp.Marketplace.Name,
                               CustomerId = mp.Customer.Id,
                               ErrorMessage = mp.UpdateError,
                               Name = mp.DisplayName,
                               UpdateStartDate = mp.UpdatingStart,
                               Status = mp.GetUpdatingStatus(null)
                           }).ToList();

            switch (sortField)
            {
                case SortField.Umi:
                    models = sortType == "asc"
                        ? models.OrderBy(x => x.Umi).ToList()
                        : models.OrderByDescending(x => x.Umi).ToList();
                    break;
                case SortField.MpType:
                    models = sortType == "asc"
                        ? models.OrderBy(x => x.Type).ToList()
                        : models.OrderByDescending(x => x.Type).ToList();
                    break;
                case SortField.MpName:
                    models = sortType == "asc"
                        ? models.OrderBy(x => x.Name).ToList()
                        : models.OrderByDescending(x => x.Name).ToList();
                    break;
                case SortField.ErrorMessage:
                    models = sortType == "asc"
                        ? models.OrderBy(x => x.ErrorMessage).ToList()
                        : models.OrderByDescending(x => x.ErrorMessage).ToList();
                    break;
                case SortField.CustomerId:
                    models = sortType == "asc"
                        ? models.OrderBy(x => x.CustomerId).ToList()
                        : models.OrderByDescending(x => x.CustomerId).ToList();
                    break;
                case SortField.Status:
                    models = sortType == "asc"
                        ? models.OrderBy(x => x.Status).ToList()
                        : models.OrderByDescending(x => x.Status).ToList();
                    break;

                //case SortField.UpdateStartDate:
                default:
                    models = sortType == "asc"
                        ? models.OrderBy(x => x.UpdateStartDate).ToList()
                        : models.OrderByDescending(x => x.UpdateStartDate).ToList();
                    break;
            }
            return this.JsonNet(new {models});
        }
    }
}