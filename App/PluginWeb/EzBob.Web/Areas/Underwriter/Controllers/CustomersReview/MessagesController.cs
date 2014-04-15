namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Data;
	using System.IO;
	using System.Web.Mvc;
	using Aspose.Words;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using StructureMap;

	public class MessagesController : Controller
	{
		private readonly ServiceClient m_oServiceClient;
		private readonly CustomerRepository _customersRepository;
		private readonly IWorkplaceContext _workplaceContext;
		private readonly IDecisionHistoryRepository _historyRepository;
		private readonly MessagesModelBuilder _builder;
		private readonly ExportResultRepository _exportResultRepository;
		private readonly AskvilleRepository _askvilleRepository;

		public MessagesController(
			CustomerRepository customers,
			ExportResultRepository exportResultRepository,
			AskvilleRepository askvilleRepository,
			IDecisionHistoryRepository historyRepository,
			MessagesModelBuilder builder
		)
		{
			m_oServiceClient = new ServiceClient();
			_exportResultRepository = exportResultRepository;
			_askvilleRepository = askvilleRepository;
			_historyRepository = historyRepository;
			_builder = builder;
			_customersRepository = customers;
			_workplaceContext = ObjectFactory.GetInstance<IWorkplaceContext>();
		}

		[HttpGet]
		public JsonResult Index(int id)
		{
			var customer = _customersRepository.Get(id);
			var model = _builder.Create(customer);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		public FileResult DownloadMessagesDocument(string id)
		{
			Guid guid;
			FileResult fs;
			if (Guid.TryParse(id, out guid))
			{
				var askville = _askvilleRepository.GetAskvilleByGuid(id);
				var askvilleData = ConvertFormat(string.IsNullOrEmpty(askville.MessageBody) ? "" : askville.MessageBody, SaveFormat.Docx, "text");
				fs = new FileContentResult(askvilleData, "application/msoffice")
						 {
							 FileDownloadName =
								 string.Format(
									 "Askville({0}).doc",
									 FormattingUtils
										 .FormatDateTimeToStringWithoutSpaces
										 (askville.CreationDate))
						 };
			}
			else
			{
				var f = _exportResultRepository.Get(Convert.ToInt32(id));
				if (f == null)
				{
					throw new Exception(String.Format("File id={0} not found", id));
				}
				fs = new FileContentResult(f.BinaryBody, f.FileType == 1 ? "application/pdf" : "application/msoffice");
				fs.FileDownloadName = f.FileName.Replace(",", "").Replace("£", "");
			}
			return fs;
		}

		private byte[] ConvertFormat(string stringForConvert, SaveFormat format, string typeInputString = "html")
		{
			var doc = new Document();
			var docBuilder = new DocumentBuilder(doc);
			if (typeInputString == "html")
			{
				docBuilder.InsertHtml(stringForConvert);
			}
			else
			{
				docBuilder.Write(stringForConvert);
			}

			using (var streamForDoc = new MemoryStream())
			{
				doc.Save(streamForDoc, format);
				return streamForDoc.ToArray();
			}
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[HttpPost]
		[Ajax]
		public void MoreAMLInformation(int id)
		{
			var customer = _customersRepository.Get(id);
			m_oServiceClient.Instance.MoreAmlInformation(_workplaceContext.User.Id, customer.Id);
			customer.CreditResult = CreditResultStatus.ApprovedPending;
			customer.PendingStatus = PendingStatus.AML;
			LogPending(customer, PendingStatus.AML);
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[HttpPost]
		[Ajax]
		public void MoreAMLandBWAInformation(int id)
		{
			var customer = _customersRepository.Get(id);
			m_oServiceClient.Instance.MoreAmlAndBwaInformation(_workplaceContext.User.Id, customer.Id);
			customer.CreditResult = CreditResultStatus.ApprovedPending;
			customer.PendingStatus = PendingStatus.Bank_AML;
			LogPending(customer, PendingStatus.Bank_AML);
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[HttpPost]
		[Ajax]
		public void MoreBWAInformation(int id)
		{
			var customer = _customersRepository.Get(id);
			m_oServiceClient.Instance.MoreBwaInformation(_workplaceContext.User.Id, customer.Id);
			customer.CreditResult = CreditResultStatus.ApprovedPending;
			customer.PendingStatus = PendingStatus.Bank;
			LogPending(customer, PendingStatus.Bank);
		}

		private void LogPending(Customer customer, PendingStatus status)
		{
			var workplaceContext = ObjectFactory.GetInstance<IWorkplaceContext>();
			var user = workplaceContext.User;
			_historyRepository.LogAction(DecisionActions.Pending, status.ToString(), user, customer);
		}
	}
}
