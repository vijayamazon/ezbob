using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Repository;
using EzBob.Configuration;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models.CAIS;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class CAISController : Controller
    {
        private readonly IAppCreator _appCreator;
        private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;
        private readonly ConfigurationRootBob _config;
        private readonly IWorkplaceContext _context;

        public CAISController(IAppCreator appCreator, IWorkplaceContext context, ConfigurationRootBob config,
                              CaisReportsHistoryRepository caisReportsHistoryRepository)
        {
            _appCreator = appCreator;
            _context = context;
            _config = config;
            _caisReportsHistoryRepository = caisReportsHistoryRepository;
        }

        public ActionResult Index()
        {
            ViewData["CAISFilesLocationPath"] = _config.Experian.CAISFilesLocationPath;
            return View();
        }

        [Ajax]
        [HttpPost]
        public void Generate()
        {
            _appCreator.CAISGenerate(_context.User,
                _config.Experian.CAISFilesLocationPath,
                _config.Experian.CAISFilesLocationPath2);
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult ListOfFiles()
        {
            string path = _config.Experian.CAISFilesLocationPath;
            var cais = CaisModel.FromModel(_caisReportsHistoryRepository.GetAll());
            return this.JsonNet(new {cais, path});
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