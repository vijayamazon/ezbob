namespace ExperianLib.Ebusiness {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class LimitedResults : BusinessReturnData {
		#region public

		#region properties
		
		public decimal ExistingBusinessLoans { get; set; }
		public SortedSet<string> Owners { get; protected set; }

		#endregion properties

		#region constructors

		public LimitedResults(ExperianLtd oExperianLtd, bool bCacheHit) : base(oExperianLtd.ServiceLogID, oExperianLtd.ReceivedTime, bCacheHit) {
			Owners = new SortedSet<string>();
			RawExperianLtd = oExperianLtd;
			Parse();
		} // constructor

		public LimitedResults(Exception exception) : base(exception) {
			Owners = new SortedSet<string>();
		} // constructor

		#endregion constructors

		#region property IsLimited

		public override bool IsLimited {
			get { return true; }
		} // IsLimited

		#endregion property IsLimited

		#region property RawExperianLtd

		public virtual ExperianLtd RawExperianLtd { get; private set; }

		#endregion property RawExperianLtd

		#endregion public

		#region private

		private void Parse() {
			BureauScore = RawExperianLtd.CommercialDelphiScore.HasValue
				? RawExperianLtd.CommercialDelphiScore.Value
				: 0;

			if (RawExperianLtd.CommercialDelphiCreditLimit.HasValue)
				CreditLimit = RawExperianLtd.CommercialDelphiCreditLimit.Value;

			CompanyName = RawExperianLtd.CompanyName;

			AddressLine1 = RawExperianLtd.OfficeAddress1;
			AddressLine2 = RawExperianLtd.OfficeAddress2;
			AddressLine3 = RawExperianLtd.OfficeAddress3;
			AddressLine4 = RawExperianLtd.OfficeAddress4;
			PostCode = RawExperianLtd.OfficeAddressPostcode;

			if (Owners == null)
				Owners = new SortedSet<string>();

			if (!string.IsNullOrWhiteSpace(RawExperianLtd.RegisteredNumberOfTheCurrentUltimateParentCompany))
				Owners.Add(RawExperianLtd.RegisteredNumberOfTheCurrentUltimateParentCompany.Trim());

			ExistingBusinessLoans = 0;

			var oErrors = new List<string>();

			foreach (var oKid in RawExperianLtd.Children) {
				if (oKid.GetType() == typeof (ExperianLtdShareholders)) {
					ExperianLtdShareholders obj = (ExperianLtdShareholders)oKid;

					if (!string.IsNullOrWhiteSpace(obj.RegisteredNumberOfALimitedCompanyWhichIsAShareholder))
						Owners.Add(obj.RegisteredNumberOfALimitedCompanyWhichIsAShareholder.Trim());
				}
				else if (oKid.GetType() == typeof (ExperianLtdCaisMonthly)) {
					ExperianLtdCaisMonthly obj = (ExperianLtdCaisMonthly)oKid;

					if (obj.NumberOfActiveAccounts.HasValue)
						ExistingBusinessLoans += obj.NumberOfActiveAccounts.Value;
				}
				else if (oKid.GetType() == typeof (ExperianLtdErrors)) {
					ExperianLtdErrors obj = (ExperianLtdErrors)oKid;

					if (!string.IsNullOrWhiteSpace(obj.ErrorMessage))
						oErrors.Add(obj.ErrorMessage);
				}
			} // for

			if (oErrors.Count > 0)
				Error += string.Join("", oErrors);

			IncorporationDate = RawExperianLtd.IncorporationDate;

			IsDataExpired = LastCheckDate.HasValue &&
				(DateTime.UtcNow - LastCheckDate.Value).TotalDays >= CurrentValues.Instance.UpdateCompanyDataPeriodDays;
		} // Parse

		#endregion private
	} // class LimitedResults
} // namespace
