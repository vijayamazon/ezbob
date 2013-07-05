using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Repository;
using ExperianLib.CaisFile;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models.CAIS;
using EzBob.Web.Code;
using Scorto.Web;
using log4net;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class CAISController : Controller
    {
        private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;
        private readonly IAppCreator _appCreator;
        private readonly IWorkplaceContext _context;
        private static readonly ILog Log = LogManager.GetLogger(typeof(CAISController));

        public CAISController(CaisReportsHistoryRepository caisReportsHistoryRepository, IAppCreator appCreator, IWorkplaceContext context)
        {
            _caisReportsHistoryRepository = caisReportsHistoryRepository;
            _appCreator = appCreator;
            _context = context;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Ajax]
        [HttpPost]
        public JsonNetResult Generate()
        {
            try
            {
                _appCreator.CAISGenerate(_context.User);
            }
            catch (Exception e)
            {
                return this.JsonNet(new {error = e});
            }
            return null;
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
        public string GetOneFile(int id)
        {
            var cais = _caisReportsHistoryRepository.Get(id);
            if (cais == null)
            {
                throw new FileNotFoundException();
            }
            return ZipString.Unzip(cais.FileData);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public void SaveFileChange(string fileContent, int id)
        {
            _caisReportsHistoryRepository.UpdateFile(ZipString.Zip(fileContent), id);
            _appCreator.CAISUpdate(_context.User, id);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonNetResult SendFiles(IEnumerable<CaisSendModel> model)
        {
            var error = new HashSet<string>();
            foreach (var el in model)
            {
                try
                {
                    SendCAISFile(el.Id);
                }
                catch (Exception e)
                {
                    error.Add(e.Message + (e.InnerException != null ? " Inner exception: "+e.InnerException.Message : ""));
                }
            }
            return error.Count > 0 ? this.JsonNet(string.Join(Environment.NewLine, error)) : null;
        }

        private void SendCAISFile(int id)
        {
            var file = _caisReportsHistoryRepository.Get(id);
            var sender = new CaisFileSender();
            
            try
            {
                sender.UploadData(ZipString.Unzip(file.FileData), file.FileName);
                file.UploadStatus = CaisUploadStatus.Uploaded;
            }
            catch (Exception e)
            {
                file.UploadStatus = CaisUploadStatus.UploadError;
                throw;
            }
        }
    }
}