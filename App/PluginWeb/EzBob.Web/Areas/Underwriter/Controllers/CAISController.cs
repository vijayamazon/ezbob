namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib.CaisFile;
	using Ezbob.Backend.Models;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models.CAIS;
	using ServiceClientProxy;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class CAISController : Controller
	{
		private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;
		private readonly ServiceClient m_oServiceClient;
		private readonly IWorkplaceContext _context;

		public CAISController(CaisReportsHistoryRepository caisReportsHistoryRepository, IWorkplaceContext context)
		{
			_caisReportsHistoryRepository = caisReportsHistoryRepository;
			m_oServiceClient = new ServiceClient();
			_context = context;
		}

		public ActionResult Index()
		{
			return View();
		}

		[Ajax]
		[HttpPost]
		[Permission(Name = "GenerateCAIS")]
		public JsonResult Generate()
		{
			try
			{
				m_oServiceClient.Instance.CaisGenerate(_context.User.Id);
			}
			catch (Exception e)
			{
				return Json(new { error = e });
			}

			return null;
		}

		[HttpGet]
		public FileResult DownloadFile(int id)
		{
			var cais = _caisReportsHistoryRepository.Get(id);
			var bytes = Encoding.UTF8.GetBytes(ZipString.Unzip(cais.FileData));
			var result = new FileContentResult(bytes, "text/plain") { FileDownloadName = cais.FileName };
			return result;
		}

		[Ajax]
		[HttpGet]
		public JsonResult ListOfFiles() {
			var cais = _caisReportsHistoryRepository
				.GetAll()
				.OrderByDescending(x => x.Date)
				.Select(CaisModel.FromModel)
				.ToList();

			return Json(new { cais }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public string GetOneFile(int id)
		{
			var cais = _caisReportsHistoryRepository.Get(id);
			if (cais == null)
				throw new FileNotFoundException();

			return ZipString.Unzip(cais.FileData);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "EditCAISFile")]
		public void SaveFileChange(string fileContent, int id)
		{
			_caisReportsHistoryRepository.UpdateFile(ZipString.Zip(fileContent), id);
			m_oServiceClient.Instance.CaisUpdate(_context.User.Id, id);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "EditCAISFile")]
		public void UpdateStatus(int id)
		{
			_caisReportsHistoryRepository.Get(id).UploadStatus = CaisUploadStatus.Uploaded;
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SendFiles(IEnumerable<CaisSendModel> model)
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
					error.Add(e.Message + (e.InnerException != null ? " Inner exception: " + e.InnerException.Message : ""));
				}
			}

			return error.Count > 0 ? Json(string.Join(Environment.NewLine, error), JsonRequestBehavior.AllowGet) : null;
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
			catch (Exception)
			{
				file.UploadStatus = CaisUploadStatus.UploadError;
				throw;
			}
		}
	}
}