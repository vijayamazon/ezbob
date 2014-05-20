﻿namespace Reports {
	using EZBob.DatabaseLib.Model.Database;
	using Newtonsoft.Json.Linq;
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;

	internal class LoanStatsDataEntry {
		#region public

		#region properties - loaded from database

		public int CustomerID { get; private set; }
		public string ApprovedType { get; private set; }
		public bool IsLoanTypeSelectionAllowed { get; private set; }
		public string DiscountPlanName { get; private set; }
		public bool IsOffline { get; private set; }
		public string CustomerName { get; private set; }
		public DateTime FirstDecisionDate { get; private set; }
		public DateTime LastDecisionDate { get; private set; }
		public Decimal ApprovedSum { get; private set; }
		public Decimal ApprovedRate { get; private set; }
		public int CreditScore { get; private set; }
		public int AnnualTurnover { get; private set; }
		public Medal Medal { get; private set; }
		public Gender Gender { get; private set; }
		public DateTime BirthDate { get; private set; }
		public MaritalStatus MaritalStatus { get; private set; }
		public string ResidentialStatus { get; private set; }
		public TypeOfBusiness TypeOfBusiness { get; private set; }
		public string ReferenceSource { get; private set; }
		public int LoanID { get; private set; }
		public Decimal LoanAmount { get; private set; }
		public DateTime IssueDate { get; private set; }
		public int LoanTerm { get; private set; }
		public List<int> RequestIDHistory { get; private set; }

		#endregion properties - loaded from database

		#region properties - calculated

		public bool IsLoanIssued { get { return LoanID != 0; } }
		public bool IsNewClient { get { return LoanSeqNo == 1; } }
		public int LoanSeqNo { get; set; }

		#endregion properties - calculated

		#region consturctor

		public LoanStatsDataEntry(SafeReader sr) {
			RequestIDHistory = new List<int>();
			LoanTerm = 0;
			Update(sr);
			FirstDecisionDate = LastDecisionDate;
		} // consturctor

		#endregion consturctor

		#region method Update

		public void Update(SafeReader sr) {
			CustomerID = sr["CustomerID"];

			RequestIDHistory.Add(sr["RequestID"]);

			string sLoanType = sr["LoanType"].ToString().ToLower();

			switch (sLoanType) {
			case "standardloantype":
				ApprovedType = "s";
				break;
			case "halfwayloantype":
				ApprovedType = "n";
				break;
			default:
				throw new ArgumentOutOfRangeException("Unsupported loan type: " + sLoanType);
			} // switch

			IsLoanTypeSelectionAllowed = sr["IsLoanTypeSelectionAllowed"];
			DiscountPlanName = sr["DiscountPlanName"];
			IsOffline = sr["IsOffline"];
			CustomerName = sr["CustomerName"];
			LastDecisionDate = sr["DecisionDate"];
			ApprovedSum = sr["ApprovedSum"];
			ApprovedRate = sr["ApprovedRate"];
			CreditScore = sr["CreditScore"];
			AnnualTurnover = sr["AnnualTurnover"];

			string sMedalType = sr["MedalType"];
			if (string.IsNullOrEmpty(sMedalType))
				sMedalType = Medal.Silver.ToString();

			Medal = (Medal)Enum.Parse(typeof(Medal), sMedalType);

			Gender = (Gender)Enum.Parse(typeof(Gender), sr["Gender"]);
			BirthDate = sr["DateOfBirth"];
			MaritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), sr["MaritalStatus"]);
			ResidentialStatus = sr["ResidentialStatus"];
			TypeOfBusiness = (TypeOfBusiness)Enum.Parse(typeof(TypeOfBusiness), sr["TypeOfBusiness"]);
			ReferenceSource = sr["ReferenceSource"];
			LoanID = sr["LoanID"];
			LoanAmount = sr["LoanAmount"];
			IssueDate = sr["LoanIssueDate"];

			if (LoanID != 0) {
				JObject jo = JObject.Parse(sr["AgreementModel"]);
				LoanTerm = (int)jo["Term"];
			} // if
		} // Update

		#endregion method Update

		#endregion public
	} // class LoanStatsDataEntry
} // namespace
