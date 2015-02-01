using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;

namespace EzBob.Web.Areas.Underwriter.Models
{
	using Ezbob.Backend.Models;

	public class MessagesModelBuilder
    {
        private readonly EzbobMailNodeAttachRelationRepository _ezbobMailNodeAttachRelationRepository;
        private readonly AskvilleRepository _askvilleRepository;

        public MessagesModelBuilder(EzbobMailNodeAttachRelationRepository ezbobMailNodeAttachRelationRepository, AskvilleRepository askvilleRepository)
        {
            _ezbobMailNodeAttachRelationRepository = ezbobMailNodeAttachRelationRepository;
            _askvilleRepository = askvilleRepository;
        }

        public List<MessagesModel> Create(EZBob.DatabaseLib.Model.Database.Customer customer) {
	        var attach = _ezbobMailNodeAttachRelationRepository.GetAll()
		        .Where(x => x.To == customer.Name)
		        .Select(val => new MessagesModel {
			        Id = val.Export.Id.ToString(CultureInfo.InvariantCulture),
			        CreationDate = FormattingUtils.FormatDateTimeToString(val.Export.CreationDate, ""),
			        FileName = val.Export.FileName
		        })
		        .ToList();

	        var askvilleMessages = _askvilleRepository
		        .GetAskvilleByCustomerId(customer.Id)
		        .Select(x => new MessagesModel {
			        Id = x.Guid,
			        CreationDate = FormattingUtils.FormatDateTimeToStringWithoutSpaces(x.CreationDate),
			        FileName = string.Format("Askville({0}).docx", FormattingUtils.FormatDateTimeToStringWithoutSpaces(x.CreationDate))
		        })
		        .ToList();

            var model = new List<MessagesModel>();
            model.AddRange(attach);
            model.AddRange(askvilleMessages);

            return model;
        }
    }
}