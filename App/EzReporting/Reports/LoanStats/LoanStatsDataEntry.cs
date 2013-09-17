using EZBob.DatabaseLib.Model.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace Reports {
	internal class LoanStatsDataEntry {
		#region public

		#region properties - loaded from database

		public int CustomerID { get; private set; }
		public string ApprovedType { get; private set; }
		public bool IsLoanTypeSelectionAllowed { get; private set; }
		public string DiscountPlanName { get; private set; }
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
		public MartialStatus MaritalStatus { get; private set; }
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

		public bool IsLoanIssued { get { return this.LoanID != 0; } }
		public bool IsNewClient { get { return this.LoanSeqNo == 1; } }
		public int LoanSeqNo { get; set; }

		#endregion properties - calculated

		#region consturctor

		public LoanStatsDataEntry(DataRow row) {
			RequestIDHistory = new List<int>();
			LoanTerm = 0;
			Update(row);
			FirstDecisionDate = LastDecisionDate;
		} // consturctor

		#endregion consturctor

		#region method Update

		public void Update(DataRow row) {
			CustomerID = Convert.ToInt32(row["CustomerID"]);

			RequestIDHistory.Add(Convert.ToInt32(row["RequestID"]));

			string sLoanType = row["LoanType"].ToString().ToLower();

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

			IsLoanTypeSelectionAllowed = Convert.ToBoolean(row["IsLoanTypeSelectionAllowed"]);
			DiscountPlanName = row["DiscountPlanName"].ToString();
			CustomerName = row["CustomerName"].ToString();
			LastDecisionDate = Convert.ToDateTime(row["DecisionDate"]);
			ApprovedSum = Convert.ToDecimal(row["ApprovedSum"]);
			ApprovedRate = Convert.ToDecimal(row["ApprovedRate"]);
			CreditScore = Convert.ToInt32(row["CreditScore"]);
			AnnualTurnover = Convert.ToInt32(row["AnnualTurnover"]);

			string sMedalType = row["MedalType"].ToString();
			if (string.IsNullOrEmpty(sMedalType))
				sMedalType = Medal.Silver.ToString();

			Medal = (Medal)Enum.Parse(typeof(Medal), sMedalType);

			Gender = (Gender)Enum.Parse(typeof(Gender), row["Gender"].ToString());
			BirthDate = Convert.ToDateTime(row["DateOfBirth"]);
			MaritalStatus = (MartialStatus)Enum.Parse(typeof(MartialStatus), row["MartialStatus"].ToString());
			ResidentialStatus = row["ResidentialStatus"].ToString();
			TypeOfBusiness = (TypeOfBusiness)Enum.Parse(typeof(TypeOfBusiness), row["TypeOfBusiness"].ToString());
			ReferenceSource = row["ReferenceSource"].ToString();
			LoanID = Convert.ToInt32(row["LoanID"]);
			LoanAmount = Convert.ToDecimal(row["LoanAmount"]);
			IssueDate = Convert.ToDateTime(row["LoanIssueDate"]);

			if (LoanID != 0) {
				string sAgreementModel = row["AgreementModel"].ToString();
				JObject jo = JObject.Parse(sAgreementModel);
				LoanTerm = (int)jo["Term"];
			} // if
		} // Update

		#endregion method Update

		#endregion public

		#region private

		private readonly CultureInfo ms_oCulture = CultureInfo.InvariantCulture;
		private const string ms_sDateFormat = "MMM dd yyyy HH:mm:ss";

		#endregion private
	} // class LoanStatsDataEntry
} // namespace
