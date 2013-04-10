using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using EZBob.DatabaseLib.Repository;
using ExperianLib.CaisFile;
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

        [Ajax]
        [HttpPost]
        [NoCache]
        public void SaveFileChange(string fileContent, string fullFileName)
        {
            using (var file = new StreamWriter(fullFileName))
            {
                file.Write(fileContent);
            }
        }

        [Ajax]
        [HttpPost]
        public void SendFiles(IEnumerable<CaisSendModel> model)
        {
            var sender = new CaisFileSender();
            var error = new HashSet<string>();
            foreach (var el in model)
            {
                using (var file = new StreamReader(el.Path))
                {
                    try
                    {
                        sender.UploadData(file.ReadToEnd(), el.Path);
                    }
                    catch (Exception e)
                    {
                        error.Add(e.Message);
                    }
                }
            }
            if (error.Count > 0)
            {
                throw new Exception(string.Join(Environment.NewLine, error));
            }
        }
    }
}