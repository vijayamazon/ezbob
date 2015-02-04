using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.Repository;

namespace EzBob.Web.Areas.Underwriter.Models
{
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib.Model.Database;

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
			var atach = _ezbobMailNodeAttachRelationRepository.GetAll().Where(x => x.To == customer.Name && x.Export.FileType == 0).ToList();
			var askvilleMessages = _askvilleRepository.GetAskvilleByCustomerId(customer.Id).ToList();

			var model = new List<MessagesModel>();
			model.AddRange(atach.Select(val => new MessagesModel {
				Id = val.Export.Id.ToString(CultureInfo.InvariantCulture),
				CreationDate = FormattingUtils.FormatDateTimeToString(val.Export.CreationDate),
				FileName = val.Export.FileName
			}));

			model.AddRange(askvilleMessages.Select(x => new MessagesModel {
				Id = x.Guid,
				CreationDate = FormattingUtils.FormatDateTimeToStringWithoutSpaces(x.CreationDate),
				FileName = string.Format("Askville({0}).docx", FormattingUtils.FormatDateTimeToStringWithoutSpaces(x.CreationDate))
			}));

			return model;
        }
    }
}