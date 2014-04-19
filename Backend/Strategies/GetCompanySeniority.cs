namespace EzBob.Backend.Strategies 
{
	using System;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.XmlUtils;

	public class GetCompanySeniority : AStrategy
	{
		private readonly int customerId;

		public GetCompanySeniority(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Get company seniority"; }
		}

		public DateTime? CompanyIncorporationDate { get; private set; }
		
		public override void Execute()
		{
			string companyData = null;
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					companyData = sr["CompanyData"];
					return ActionResult.SkipAll;
				},
				"GetCompanySeniority",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (!string.IsNullOrEmpty(companyData))
			{
				XmlNode companyInfo = Xml.ParseRoot(companyData);
				var experianUtils = new ExperianUtils(Log);
				CompanyIncorporationDate = experianUtils.DetectIncorporationDate(companyInfo);
			}
		}
	}
}
