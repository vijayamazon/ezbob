using System.Web.Mvc;
using EzBob.Web.Code.PostCode;
using EzBob.Web.Infrastructure;
using Scorto.Web;

namespace EzBob.Web.Controllers
{
	[Authorize]
    public class PostcodeController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;

        private readonly IPostCodeFacade _facade;
        public PostcodeController(IEzbobWorkplaceContext context, IPostCodeFacade facade)
        {
            _context = context;
            _facade = facade;
        }

        [OutputCache(VaryByParam = "postCode", Duration = 3600 * 24 * 7)]
        [Transactional]
        public JsonNetResult GetAddressFromPostCode(string postCode)
        {
            return this.JsonNet(_facade.GetAddressFromPostCode(_context.Customer, postCode));
        }

        [OutputCache(VaryByParam = "id", Duration = 3600 * 24 * 7)]
        [Transactional]
        public JsonNetResult GetFullAddressFromPostCode(string id)
        {
            return this.JsonNet(_facade.GetFullAddressFromPostCode(_context.Customer, id));
        }
    }
}
