namespace Ezbob.Backend.Strategies.QuickOffer {
	using System;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Xml;
	using Experian;
	using EzServiceConfiguration;
	using EzServiceConfigurationLoader;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using Ezbob.Utils.XmlUtils;

	internal class QuickOfferData {

		public QuickOfferData(QuickOfferConfigurationData qoc, AConnection oDB, ASafeLog oLog) {
			Log = new SafeLog(oLog);
			IsValid = false;
			Cfg = qoc;
			m_oExperianUtils = new ExperianUtils(oLog);
			DB = oDB;
		} // constructor

		public void Load(SafeReader oReader) {
			FatalMsg = oReader["FatalMsg"];

			if (string.IsNullOrWhiteSpace(FatalMsg)) {
				CustomerID = oReader["CustomerID"];
				RequestedAmount = oReader["RequestedAmount"];
				CompanyRefNum = oReader["CompanyRefNum"];
				// DefaultCount = oReader["DefaultCount"];
				Aml = oReader["AmlScore"];
				FirstName = oReader["FirstName"];
				LastName = oReader["LastName"];
				Enabled = QuickOfferConfiguration.GetEnabledStatus(oReader["Enabled"]);
				FundsAvailable = oReader["FundsAvailable"];
				LoanCount = oReader["LoanCount"];
				IssuedAmount = oReader["IssuedAmount"];
				OpenCashRequests = oReader["OpenCashRequests"];
				ErrorMsg = oReader["ErrorMsg"];
			} // if

			Validate();
		} // Load

		public bool IsValid { get; private set; } // IsValid

		public QuickOfferModel GetOffer(bool bSaveOfferToDB, AConnection oDB, ASafeLog oLog) {
			if (RequestedAmount < Cfg.MinOfferAmount) {
				oLog.Debug("Requested amount (£{0}) is less than minimal offer amount (£{1}), not offering.", RequestedAmount, Cfg.MinOfferAmount);
				return null;
			} // if

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					minLoanAmount = sr["MinLoanAmount"];
					return ActionResult.SkipAll;
				},
				"GetBankBasedApprovalConfigs",
				CommandSpecies.StoredProcedure
			);

			decimal? nOffer = Calculate();

			if (!nOffer.HasValue)
				return null;

			int nOfferID = default(int);

			decimal nRequestedAmount = RequestedAmount.Min(Cfg.PotentialMaxAmount);

			var oOffer = new QuickOfferModel {
				ID = nOfferID,
				Amount = nOffer.Value,
				Aml = Aml,
				BusinessScore = BusinessScore,
				IncorporationDate = IncorporationDate.Value,
				TangibleEquity = TangibleEquity,
				TotalCurrentAssets = TotalCurrentAssets,

				ImmediateTerm = Cfg.ImmediateTermMonths,
				ImmediateInterestRate = Cfg.ImmediateInterestRate,
				ImmediateSetupFee = Cfg.ImmediateSetupFee,

				PotentialAmount = nRequestedAmount,
				PotentialTerm = Cfg.PotentialTermMonths,
				PotentialInterestRate = Cfg.LoanPct(BusinessScore, nRequestedAmount),
				PotentialSetupFee = Cfg.PotentialSetupFee,
			};

			if (bSaveOfferToDB && (Cfg.Enabled != QuickOfferEnabledStatus.Silent)) {
				try {
					var oID = new QueryParameter("@QuickOfferID") {
						Type = DbType.Int32,
						Direction = ParameterDirection.Output,
					};

					oDB.ExecuteNonQuery(
						"QuickOfferSave",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@Now", DateTime.UtcNow),
						new QueryParameter("@CustomerID", CustomerID),
						new QueryParameter("@Amount", oOffer.Amount),
						new QueryParameter("@Aml", oOffer.Aml),
						new QueryParameter("@BusinessScore", oOffer.BusinessScore),
						new QueryParameter("@IncorporationDate", oOffer.IncorporationDate),
						new QueryParameter("@TangibleEquity", oOffer.TangibleEquity),
						new QueryParameter("@TotalCurrentAssets", oOffer.TotalCurrentAssets),

						new QueryParameter("@ImmediateTerm", oOffer.ImmediateTerm),
						new QueryParameter("@ImmediateInterestRate", oOffer.ImmediateInterestRate),
						new QueryParameter("@ImmediateSetupFee", oOffer.ImmediateSetupFee),
						new QueryParameter("@PotentialAmount", oOffer.PotentialAmount),
						new QueryParameter("@PotentialTerm", oOffer.PotentialTerm),
						new QueryParameter("@PotentialInterestRate", oOffer.PotentialInterestRate),
						new QueryParameter("@PotentialSetupFee", oOffer.PotentialSetupFee),
						oID
					);

					if (int.TryParse(oID.SafeReturnedValue, out nOfferID)) {
						oLog.Msg("Quick offer id is {0}", nOfferID);
						oOffer.ID = nOfferID;
					}
					else
						oLog.Warn("Failed to parse quick offer id from {0}", oID.Value.ToString());
				}
				catch (Exception e) {
					oLog.Alert(e, "Failed to save a quick offer to DB.");
				} // try
			} // if

			return oOffer;
		} // GetOffer

		private int CustomerID;
		private decimal RequestedAmount;
		private string CompanyRefNum;
		// private int DefaultCount;
		private int Aml;
		private string FirstName;
		private string LastName;

		private QuickOfferEnabledStatus Enabled;
		private decimal FundsAvailable;
		private int LoanCount;
		private decimal IssuedAmount;
		private decimal OpenCashRequests;
		private string ErrorMsg;
		private int minLoanAmount;

		private string FatalMsg;

		private int BusinessScore;
		private DateTime? IncorporationDate;
		private decimal TangibleEquity;
		private decimal TotalCurrentAssets;

		private readonly SafeLog Log;

		private readonly QuickOfferConfigurationData Cfg;

		private decimal? Calculate() {
			decimal nPct = Cfg.OfferAmountPct(BusinessScore);

			if (nPct == 0) {
				Log.Debug("No percent found for business score {0}.", BusinessScore);
				return null;
			} // if

			Log.Debug("Percent for business score {0} is {1}.", BusinessScore, nPct.ToString("P2"));

			decimal nCalculatedOffer = TotalCurrentAssets * nPct;

			var ci = new CultureInfo("en-GB", false);

			Log.Debug("Calculated offer (total current assets * percent) is {0}", nCalculatedOffer.ToString("C2", ci));

			nCalculatedOffer = (int)(Math.Round(nCalculatedOffer / minLoanAmount, 0, MidpointRounding.AwayFromZero) * minLoanAmount);
			Log.Debug("Rounded offer is {0}", nCalculatedOffer.ToString("C2", ci));

			if (nCalculatedOffer < Cfg.MinOfferAmount) {
				Log.Debug("The offer is less than {0}, not offering.", Cfg.MinOfferAmount.ToString("C2", ci));
				return null;
			} // if

			decimal nOfferBeforeCap = (int)(Math.Round((RequestedAmount * Cfg.OfferCapPct) / minLoanAmount, 0, MidpointRounding.AwayFromZero) * minLoanAmount);

			decimal nCap = nOfferBeforeCap.Min(Cfg.ImmediateMaxAmount);

			Log.Debug(
				"Offer cap is {0} = min({1}, {2} * {4} = {3})",
				nCap.ToString("C2", ci),
				Cfg.ImmediateMaxAmount.ToString("C2", ci),
				RequestedAmount.ToString("C2", ci),
				nOfferBeforeCap.ToString("C2", ci),
				Cfg.OfferCapPct.ToString("P2", ci)
			);

			decimal nOffer = nCap.Min(nCalculatedOffer);

			Log.Debug(
				"And the offer is {0} = min({1}, {2})",
				nOffer.ToString("C2", ci),
				nCalculatedOffer.ToString("C2", ci),
				nCap.ToString("C2", ci)
			);

			if (nOffer > FundsAvailable - OpenCashRequests) {
				Log.Debug(
					"The offer is withdrawn: not enough funds available: offer = {0}, funds = {1}.",
					nOffer.ToString("C2", ci),
					(FundsAvailable - OpenCashRequests).ToString("C2", ci)
				);
				return null;
			} // if

			return nOffer;
		} // Calculate

		private void Validate() {
			if (!string.IsNullOrWhiteSpace(FatalMsg)) {
				Log.Debug("QuickOffer.Validate: fatal (for quick offer calculation) error reported when selecting data from DB: {0}", FatalMsg);
				return;
			} // if

			if (!AreLoadedValid())
				return;

			if (IsThinFile())
				return;

			var oLoader = new LoadExperianLtd(CompanyRefNum, 0);
			oLoader.Execute();

			if (oLoader.Result.RegisteredNumber != CompanyRefNum)
				return;

			if (!m_oExperianUtils.IsDirector(oLoader.Result, FirstName, LastName)) {
				Log.Debug("QuickOffer.Validate: the customer is not director of this company.");
				return;
			} // if

			BusinessScore = oLoader.Result.GetCommercialDelphiScore();
			if (BusinessScore < Cfg.BusinessScoreMin) {
				Log.Debug("QuickOffer.Validate: business score is too low.");
				return;
			} // if

			IncorporationDate = oLoader.Result.IncorporationDate;
			if (!IncorporationDate.HasValue) {
				Log.Debug("QuickOffer.Validate: business age cannot be detected.");
				return;
			} // if

			if (DateTime.UtcNow.Subtract(IncorporationDate.Value).TotalDays < 30.45 * Cfg.CompanySeniorityMonths) {
				Log.Debug("QuickOffer.Validate: business is too young.");
				return;
			} // if

			decimal nTangibleEquity;
			decimal nTotalCurrentAssets;

			m_oExperianUtils.DetectTangibleEquity(oLoader.Result, out nTangibleEquity, out nTotalCurrentAssets);

			TangibleEquity = nTangibleEquity;
			TotalCurrentAssets = nTotalCurrentAssets;

			if (TangibleEquity < 0) {
				Log.Debug("QuickOffer.Validate: tangible equity is to low.");
				return;
			} // if

			IsValid = true;
		} // Validate

		private bool AreLoadedValid() {
			if (!string.IsNullOrWhiteSpace(ErrorMsg)) {
				Log.Debug("Error reported when selecting data from DB: {0}", ErrorMsg);
				return false;
			} // if

			if (Enabled == QuickOfferEnabledStatus.Disabled) {
				Log.Debug("Quick offer is disabled.");
				return false;
			} // if

			if (FundsAvailable <= 0) {
				Log.Debug("No funds available.");
				return false;
			} // if

			if (FundsAvailable <= OpenCashRequests) {
				Log.Debug("Too many open cash requests.");
				return false;
			} // if

			if (IssuedAmount >= Cfg.MaxIssuedValuePerDay) {
				Log.Debug("Too much money already issued.");
				return false;
			} // if

			if (LoanCount >= Cfg.MaxLoanCountPerDay) {
				Log.Debug("Too many loans issued.");
				return false;
			} // if

			if (CustomerID == 0) {
				Log.Debug("Customer id not set.");
				return false;
			} // if

			if (RequestedAmount <= 0) {
				Log.Debug("Requested amount is not set.");
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(CompanyRefNum)) {
				Log.Debug("Company ref number not set.");
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(FirstName)) {
				Log.Debug("Customer first name not set.");
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(LastName)) {
				Log.Debug("Customer last name not set.");
				return false;
			} // if

			return true;
		} // AreLoadedValid

		private bool IsThinFile() {
			var experianConsumer = new LoadExperianConsumerData(CustomerID, null, null);
			experianConsumer.Execute();

			if ((experianConsumer.Result.ServiceLogId == null) || (experianConsumer.Result.Cais.Count < 1)) {
				Log.Debug("No Consumer Data or CAIS details data not found: this is a thin file.");
				return true;
			} // if

			return false;
		} // IsThinFile

		private readonly ExperianUtils m_oExperianUtils;
		private readonly AConnection DB;

	} // class QuickOfferData

} // namespace Ezbob.Backend.Strategies.QuickOffer
