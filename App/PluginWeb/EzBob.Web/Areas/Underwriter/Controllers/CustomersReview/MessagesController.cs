using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Aspose.Words;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.Signals.RenderAgreements;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using Scorto.Web;
using StructureMap;
using ZohoCRM;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class MessagesController : Controller
    {
        private readonly IAppCreator _appCreator;
        private readonly CustomerRepository _customersRepository;
        private readonly IWorkplaceContext _workplaceContext;
        private readonly EzbobMailNodeAttachRelationRepository _ezbobMailNodeAttachRelationRepository;
        private readonly ExportResultRepository _exportResultRepository;
        private readonly IZohoFacade _crm;
        private readonly AskvilleRepository _askvilleRepository;

        public MessagesController(CustomerRepository customers, 
                                  IAppCreator appCreator,  
                                  EzbobMailNodeAttachRelationRepository ezbobMailNodeAttachRelationRepository, 
                                  ExportResultRepository exportResultRepository,
                                  IZohoFacade crm, AskvilleRepository askvilleRepository)
        {
            _appCreator = appCreator;
            _ezbobMailNodeAttachRelationRepository = ezbobMailNodeAttachRelationRepository;
            _exportResultRepository = exportResultRepository;
            _crm = crm;
            _askvilleRepository = askvilleRepository;
            _customersRepository = customers;
            _workplaceContext = ObjectFactory.GetInstance<IWorkplaceContext>();
        }

        [HttpGet]
        public JsonNetResult Index(int id)
        {
            var customerName = _customersRepository.Get(id).Name;
            var atach = _ezbobMailNodeAttachRelationRepository.GetAll().Where(x => x.To == customerName && x.Export.FileType == 0).ToList();
            var askvilleMessages = _askvilleRepository.GetAskvilleByCustomerId(id).ToList();

            var model = new List<MessagesModel>();
            model.AddRange(atach.Select(val => new MessagesModel
            {
                Id = val.Export.Id.ToString(CultureInfo.InvariantCulture),
                CreationDate = FormattingUtils.FormatDateTimeToString(val.Export.CreationDate),
                FileName = val.Export.FileName
            }));

            model.AddRange(askvilleMessages.Select(x => new MessagesModel
                                                            {
                                                                Id = x.Guid,
                                                                CreationDate = FormattingUtils.FormatDateTimeToStringWithoutSpaces(x.CreationDate),
                                                                FileName = string.Format("Askville({0}).docx", FormattingUtils.FormatDateTimeToStringWithoutSpaces(x.CreationDate))
                                                            }));
            return this.JsonNet(new { attaches = model });
        }

        public FileResult DownloadMessagesDocument(string id)
        {
            Guid guid;
            FileResult fs;
            if (Guid.TryParse(id, out guid))
            {
                var askville = _askvilleRepository.GetAskvilleByGuid(id);
                var askvilleData = RenderAgreementsHandler.ConvertFormat(string.IsNullOrEmpty(askville.MessageBody) ? "": askville.MessageBody, SaveFormat.Docx, "text");
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
                fs.FileDownloadName = f.FileName;
            }
            return fs;
        }

        [Transactional]
        [HttpPost]
        [Ajax]
        public void MoreAMLInformation(int id)
        {
            var customer = _customersRepository.Get(id);
            _crm.MoreAMLInformation(customer);
            _appCreator.MoreAMLInformation(_workplaceContext.User, customer.Name, customer.Id, customer.PersonalInfo.FirstName);
        }

        [Transactional]
        [HttpPost]
        [Ajax]
        public void MoreAMLandBWAInformation(int id)
        {
            var customer = _customersRepository.Get(id);
            _crm.MoreAMLInformation(customer);
            _crm.MoreBWAInformation(customer);
            _appCreator.MoreAMLandBWAInformation(_workplaceContext.User, customer.Name, customer.Id, customer.PersonalInfo.FirstName);
        }

        [Transactional]
        [HttpPost]
        [Ajax]
        public void MoreBWAInformation(int id)
        {
            var customer = _customersRepository.Get(id);
            _crm.MoreBWAInformation(customer);
            _appCreator.MoreBWAInformation(_workplaceContext.User, customer.Name, customer.Id, customer.PersonalInfo.FirstName);
        }
    }
}
