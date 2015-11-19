namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using ExperianLib.Ebusiness;
	using Ezbob.Backend.Models;

	public class ExperianTargeting : AStrategy {
		public ExperianTargeting(ExperianTargetingRequest request) {
			this.request = request;
		} // constructor

		public override string Name {
			get { return "Experian Targeting"; }
		} // Name

		public List<CompanyInfo> Response { get; set; }

		public override void Execute() {
			var service = new EBusinessService(DB);

			try {
				var nFilter = TargetResults.LegalStatus.DontCare;

				switch (this.request.Filter.ToUpper()) {
				case "L":
					nFilter = TargetResults.LegalStatus.Limited;
					break;

				case "N":
					nFilter = TargetResults.LegalStatus.NonLimited;
					break;
				} // switch

				TargetResults result = service.TargetBusiness(
					this.request.CompanyName,
					this.request.Postcode,
					this.request.CustomerID,
					nFilter, this.request.RefNum
				);

				Response = result.Targets;
			} catch (WebException we) {
				Log.Debug(we, "WebException caught while executing company targeting.");

				Response = new List<CompanyInfo> { new CompanyInfo { BusName = "", BusRefNum = "exception" } };
			} catch (Exception e) {
				if (this.request.CompanyName.ToLower() == "asd" && this.request.Postcode.ToLower() == "ab10 1ba")
					Response = GenerateFakeTargetingData(this.request.CompanyName, this.request.Postcode);

				Log.Alert(e, "Target Business failed.");
			} // try
		} // Execute

		private static List<CompanyInfo> GenerateFakeTargetingData(string companyName, string postcode) {
			var data = new List<CompanyInfo>();

			for (var i = 0; i < 10; i++) {
				data.Add(new CompanyInfo {
					AddrLine1 = "AddrLine1" + " for company " + i,
					AddrLine2 = "AddrLine2" + " for company " + i,
					AddrLine3 = "AddrLine3" + " for company " + i,
					AddrLine4 = "AddrLine4" + " for company " + i,
					BusName = "BusName" + " for company " + i,
					BusRefNum = "BusRefNum" + " for company " + i,
					BusinessStatus = "BusinessStatus" + " for company " + i,
					LegalStatus = "LegalStatus" + " for company " + i,
					MatchScore = "MatchScore" + " for company " + i,
					MatchedBusName = companyName + " for company " + i,
					MatchedBusNameType = "MatchedBusNameType" + " for company " + i,
					PostCode = postcode + " for company " + i,
					SicCode = "SicCode" + " for company " + i,
					SicCodeDesc = "SicCodeDesc" + " for company " + i,
					SicCodeType = "SicCodeType" + " for company " + i
				});
			} // for

			return data;
		} // GenerateFakeTargetingData

		private readonly ExperianTargetingRequest request;
	} // class
} // namespace
