using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Mvc;
using ExperianLib.CaisFile;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Infrastructure;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class CAISController : Controller
    {
        private readonly IAppCreator _appCreator;
        private readonly IWorkplaceContext _context;
        private readonly IEzBobConfiguration _configuration;
        public CAISController(IAppCreator appCreator, IWorkplaceContext context, IEzBobConfiguration configuration)
        {
            _appCreator = appCreator;
            _context = context;
            _configuration = configuration;
        }

        public ActionResult Index()
        {
            ViewData["CAISDisplayedPath"] = _configuration.CAISDisplayedPath;
            return View();
        }

        [Ajax]
        [HttpPost]
        public void Generate()
        {
            _appCreator.CAISNOUpload(_context.User);
        }

        [Ajax]
        [HttpPost]
        public void SendCAISFile(IEnumerable<HttpPostedFileBase> files)
        {
            var sender = new CaisFileSender();
            foreach (var file in files)
            {
                if (file == null) throw new FileNotFoundException();
                sender.UploadData(file);
            }
        }
    }
}
