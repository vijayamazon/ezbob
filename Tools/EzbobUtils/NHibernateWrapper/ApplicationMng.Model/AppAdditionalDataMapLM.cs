using System;
namespace ApplicationMng.Model
{
	public class AppAdditionalDataMapLM : AppAdditionalDataMap<AppAdditionalDataMapLM>
	{
		public AppAdditionalDataMapLM()
		{
			base.Map((AppAdditionalData x) => x.Name);
			base.Map((AppAdditionalData x) => x.PassportSeries);
			base.Map((AppAdditionalData x) => x.Patronymic);
			base.Map((AppAdditionalData x) => x.Surname);
			base.Map((AppAdditionalData x) => x.CreditProduct).Length(1024);
			base.Map((AppAdditionalData x) => (object)x.DesiredCreditSum);
			base.Map((AppAdditionalData x) => (object)x.ActualCreditSum);
			base.Map((AppAdditionalData x) => x.ReadOnlyNodeName).Length(1024);
			base.Map((AppAdditionalData x) => x.AutoCreditTerm).Length(1024);
			base.Map((AppAdditionalData x) => x.AutoCreditFirstPayment).Length(1024);
		}
	}
}
