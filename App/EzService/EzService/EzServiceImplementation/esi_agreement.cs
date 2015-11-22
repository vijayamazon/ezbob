namespace EzService.EzServiceImplementation
{
	using EzBob.Backend.Models;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

    partial class EzServiceImplementation
	{
		public ActionMetaData SaveAgreement(int customerId, AgreementModel model, string refNumber, string name,
											TemplateModel template,
											string path1, string path2)
		{
			return Execute<SaveAgreement>(customerId, null, customerId, model, refNumber, name, template, path1, path2);
		}

        public NLLongActionResult AddPayment(int userID, int customerID, NL_Payments payment) {
            return null;
        }
    }
}
