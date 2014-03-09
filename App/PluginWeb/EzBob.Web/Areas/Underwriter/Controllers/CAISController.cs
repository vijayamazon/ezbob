namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib.CaisFile;
	using EzBob.Web.Areas.Underwriter.Models.CAIS;
	using EzBob.Web.Code;
	using Scorto.Web;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class CAISController : Controller {
        private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;
		private readonly ServiceClient m_oServiceClient;
        private readonly IWorkplaceContext _context;

        public CAISController(CaisReportsHistoryRepository caisReportsHistoryRepository, IWorkplaceContext context) {
            _caisReportsHistoryRepository = caisReportsHistoryRepository;
	        m_oServiceClient = new ServiceClient();
            _context = context;
        }

        public ActionResult Index() {
            return View();
        }

        [Ajax]
        [HttpPost]
        public JsonNetResult Generate() {
            try {
                m_oServiceClient.Instance.CaisGenerate(_context.User.Id);
            }
            catch (Exception e) {
                return this.JsonNet(new {error = e});
            }

            return null;
        }

        [HttpGet]
        public FileResult DownloadFile(int id) {
            var cais = _caisReportsHistoryRepository.Get(id);
            var bytes = Encoding.UTF8.GetBytes(ZipString.Unzip(cais.FileData));
            var result = new FileContentResult(bytes, "text/plain") { FileDownloadName = cais.FileName };
            return result;
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult ListOfFiles() {
            var cais = CaisModel.FromModel(_caisReportsHistoryRepository.GetAll());
            return this.JsonNet(new {cais});
        }

        [Ajax]
        [HttpGet]
        public string GetOneFile(int id) {
            var cais = _caisReportsHistoryRepository.Get(id);
            if (cais == null)
                throw new FileNotFoundException();

            return ZipString.Unzip(cais.FileData);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public void SaveFileChange(string fileContent, int id) {
            _caisReportsHistoryRepository.UpdateFile(ZipString.Zip(fileContent), id);
            m_oServiceClient.Instance.CaisUpdate(_context.User.Id, id);
        }

		[Ajax]
        [HttpPost]
        [Transactional]
		public void UpdateStatus(int id) {
			_caisReportsHistoryRepository.Get(id).UploadStatus = CaisUploadStatus.Uploaded;
		}

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonNetResult SendFiles(IEnumerable<CaisSendModel> model) {
            var error = new HashSet<string>();
            foreach (var el in model) {
                try {
                    SendCAISFile(el.Id);
                }
                catch (Exception e) {
                    error.Add(e.Message + (e.InnerException != null ? " Inner exception: "+e.InnerException.Message : ""));
                }
            }

            return error.Count > 0 ? this.JsonNet(string.Join(Environment.NewLine, error)) : null;
        }

        private void SendCAISFile(int id) {
            var file = _caisReportsHistoryRepository.Get(id);
            var sender = new CaisFileSender();
            
            try {
                sender.UploadData(ZipString.Unzip(file.FileData), file.FileName);
                file.UploadStatus = CaisUploadStatus.Uploaded;
            }
            catch (Exception) {
                file.UploadStatus = CaisUploadStatus.UploadError;
                throw;
            }
        }
    }
}