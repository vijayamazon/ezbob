namespace EzService
{
	using EzBob.Backend.Strategies;
	using EzBob.Models.Agreements;

	partial class EzServiceImplementation
	{
		public ActionMetaData SaveAgreement(int customerId, AgreementModel model, string refNumber, string name,
											TemplateModel template,
											string path1, string path2)
		{
			return Execute(customerId, null, typeof(SaveAgreement), customerId, model, refNumber, name, template, path1, path2);
		}
	}
}
