using System.Web.Mvc;
using EzBob.Web.Code.PostCode;
using EzBob.Web.Infrastructure;
using NHibernate;
using Scorto.Web;
using log4net;

namespace EzBob.Web.Controllers
{
    public class PostcodeController : Controller
    {
        
        private readonly IEzbobWorkplaceContext _context;

        private static readonly ILog _log = LogManager.GetLogger(typeof(PostcodeController));
        private readonly IPostCodeFacade _facade;
        public PostcodeController( ISession session, IEzbobWorkplaceContext context, IPostCodeFacade facade)
        {
            _context = context;
            _facade = facade;
        }

        [Authorize]
        [OutputCache(VaryByParam = "postCode", Duration = 3600 * 24 * 7)]
        [Transactional]
        public JsonNetResult GetAddressFromPostCode(string postCode)
        {
            return this.JsonNet(_facade.GetAddressFromPostCode(_context.Customer, postCode));
        }

        [Authorize]
        [OutputCache(VaryByParam = "id", Duration = 3600 * 24 * 7)]
        [Transactional]
        public JsonNetResult GetFullAddressFromPostCode(string id)
        {
            return this.JsonNet(_facade.GetFullAddressFromPostCode(_context.Customer, id));
        }
    }
}
