﻿namespace ExperianLib.Ebusiness {
	using System;
	using System.Collections.Generic;
	using System.Xml.Linq;
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class LimitedResults : BusinessReturnData {
		#region public

		#region properties
		
		public decimal ExistingBusinessLoans { get; set; }
		public SortedSet<string> Owners { get; protected set; }

		#endregion properties

		#region constructors

		public LimitedResults(ExperianLtd oExperianLtd, bool bCacheHit) : base(oExperianLtd.ServiceLogID, "<xml />", oExperianLtd.ReceivedTime) {
			Error = string.Empty;

			Owners = new SortedSet<string>();

			CacheHit = bCacheHit;
			m_oRawData = oExperianLtd;

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

		#endregion public

		#region protected

		private void Parse() {
			if (!m_oRawData.CommercialDelphiScore.HasValue)
				Error = "There is no RISKSCORE in the experian response! ";
			else
				BureauScore = m_oRawData.CommercialDelphiScore.Value;

			if (m_oRawData.CommercialDelphiCreditLimit.HasValue)
				CreditLimit = m_oRawData.CommercialDelphiCreditLimit.Value;

			CompanyName = m_oRawData.CompanyName;

			AddressLine1 = m_oRawData.OfficeAddress1;
			AddressLine2 = m_oRawData.OfficeAddress2;
			AddressLine3 = m_oRawData.OfficeAddress3;
			AddressLine4 = m_oRawData.OfficeAddress4;
			PostCode = m_oRawData.OfficeAddressPostcode;

			if (Owners == null)
				Owners = new SortedSet<string>();

			if (!string.IsNullOrWhiteSpace(m_oRawData.RegisteredNumberOfTheCurrentUltimateParentCompany))
				Owners.Add(m_oRawData.RegisteredNumberOfTheCurrentUltimateParentCompany.Trim());

			ExistingBusinessLoans = 0;

			var oErrors = new List<string>();

			foreach (var oKid in m_oRawData.Children) {
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

			IncorporationDate = m_oRawData.IncorporationDate;
		} // Parse

		#endregion protected

		#region private

		private readonly ExperianLtd m_oRawData;

		#endregion private
	} // class LimitedResults
} // namespace
