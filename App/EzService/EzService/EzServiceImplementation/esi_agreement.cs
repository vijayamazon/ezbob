namespace EzService.EzServiceImplementation
{
	using EzBob.Backend.Models;
	using EzBob.Backend.Strategies.Misc;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation
	{
		public ActionMetaData SaveAgreement(int customerId, AgreementModel model, string refNumber, string name,
											TemplateModel template,
											string path1, string path2)
		{
			return Execute<SaveAgreement>(customerId, null, customerId, model, refNumber, name, template, path1, path2);
		}
	}
}
