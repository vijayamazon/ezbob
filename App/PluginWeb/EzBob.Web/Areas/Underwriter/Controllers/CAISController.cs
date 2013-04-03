using System.IO;
using System.Web.Mvc;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models.CAIS;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class CAISController : Controller
    {
        private readonly IAppCreator _appCreator;
        private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;
        private readonly IWorkplaceContext _context;

        public CAISController(IAppCreator appCreator, IWorkplaceContext context,
                              CaisReportsHistoryRepository caisReportsHistoryRepository)
        {
            _appCreator = appCreator;
            _context = context;
            _caisReportsHistoryRepository = caisReportsHistoryRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Ajax]
        [HttpPost]
        public void Generate()
        {
            _appCreator.CAISGenerate(_context.User);
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult ListOfFiles()
        {
            var cais = CaisModel.FromModel(_caisReportsHistoryRepository.GetAll());
            return this.JsonNet(new {cais});
        }

        [Ajax]
        [HttpGet]
        public string GetOneFile(string path)
        {
            using (var file = new StreamReader(path))
            {
                return file.ReadToEnd();
            }
        }
    }
}