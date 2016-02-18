namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.ApplicationInfo;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.LogicalGlue;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Broker;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using PaymentServices.Calculators;
	using StructureMap;

	public class LoadApplicationInfo : AStrategy {
		public LoadApplicationInfo(int customerID, long? cashRequestID, DateTime? now) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
			this.now = now ?? DateTime.UtcNow;

			this.loanTypes = new List<LoanTypeModel>();
			this.discountPlans = new List<DiscountPlanModel>();
			this.loanSources = new List<LoanSourceModel>();

			Result = new ApplicationInfoModel {
				AutomationOfferModel = new AutomationOfferModel(),
			};
		} // constructor

		public override string Name {
			get { return "Load Application Info"; }
		} // Name

		public ApplicationInfoModel Result { get; private set; }

		public override void Execute() {
			DB.ForEachRowSafe(
				ProcessRow,
				"LoadApplicationInfo",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.customerID),
				new QueryParameter("@CashRequestID", this.cashRequestID),
				new QueryParameter("@Now", this.now)
			);

			Result.LoanTypes = this.loanTypes.ToArray();
			Result.AllLoanSources = this.loanSources.ToArray();
			Result.DiscountPlans = this.discountPlans.ToArray();

			BuildSuggestedAmountModel();

			BuildSetupFee();

			// Loan cost must be calculated after set up fee (it uses set up fee output).
			BuildLoanCost();

			Result.Products = DB.Fill<I_Product>("SELECT * FROM I_Product WHERE IsEnabled = 1", CommandSpecies.Text);
			Result.ProductTypes = DB.Fill<I_ProductType>("SELECT * FROM I_ProductType", CommandSpecies.Text);
			Result.ProductSubTypes = DB.Fill<I_ProductSubType>("SELECT * FROM I_ProductSubType", CommandSpecies.Text);
			Result.Grades = DB.Fill<I_Grade>("SELECT * FROM I_Grade", CommandSpecies.Text);
			Result.SubGrades = DB.Fill<I_SubGrade>("SELECT * FROM I_SubGrade", CommandSpecies.Text);
			Result.GradeRanges = DB.Fill<I_GradeRange>("SELECT * FROM I_GradeRange WHERE IsActive = 1", CommandSpecies.Text);
			Result.FundingTypes = DB.Fill<I_FundingType>("SELECT * FROM I_FundingType", CommandSpecies.Text);

			BuildLogicalGlue();
			BuildDefaultProduct();
		} // Execute

		private void BuildLogicalGlue() {
			var lg = new GetLatestKnownInference(this.customerID, this.now, false);
			lg.Execute();

			if (lg.Inference == null) { return; }

			Result.LogicalGlueScore = lg.Inference.Score;
			Result.GradeID = lg.Inference.Bucket.HasValue ? (int)lg.Inference.Bucket : (int?)null;

			if (Result.LogicalGlueScore.HasValue) {
				var subGrade = Result.SubGrades.FirstOrDefault(
					x => x.MaxScore > Result.LogicalGlueScore.Value && x.MinScore <= Result.LogicalGlueScore.Value
				);

				if (subGrade != null)
					Result.SubGradeID = subGrade.SubGradeID;
			} // if
		} // BuildLogicalGlue

		private void BuildDefaultProduct() {
			var subGrade = Result.SubGrades.FirstOrDefault(x =>
				x.GradeID == Result.GradeID &&
				x.MinScore < Result.LogicalGlueScore &&
				x.MaxScore > Result.LogicalGlueScore
			);

			if (subGrade != null)
				Result.SubGradeID = subGrade.SubGradeID;

			Result.IsRegulated = Result.TypeOfBusiness.IsRegulated();
			Result.IsLimited = Result.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited;

			CustomerOriginEnum originEnum = (CustomerOriginEnum)Result.OriginID;
			Result.Origin = originEnum.ToString();

			
			if (Result.ProductSubTypeID.HasValue) {
				Result.CurrentProductSubType = Result.ProductSubTypes.FirstOrDefault(
					x => x.ProductSubTypeID == Result.ProductSubTypeID.Value
				);
			} else {
				Result.CurrentProductSubType = Result.ProductSubTypes.FirstOrDefault(x => 
					x.OriginID == Result.OriginID && 
					x.LoanSourceID == Result.LoanSourceID &&
					x.IsRegulated == Result.IsRegulated);
			} // if

			I_ProductType currentProductType = null;

			if (Result.CurrentProductSubType != null) {
				Result.ProductSubTypeID = Result.CurrentProductSubType.ProductSubTypeID;

				currentProductType = Result.ProductTypes.FirstOrDefault(
					x => x.ProductTypeID == Result.CurrentProductSubType.ProductTypeID
				);

				Result.CurrentProductTypeID = currentProductType != null ? currentProductType.ProductTypeID : 0;

				if (Result.CurrentProductSubType.FundingTypeID.HasValue) {
					 var currentFundingType = Result.FundingTypes.FirstOrDefault(
						x => x.FundingTypeID == Result.CurrentProductSubType.FundingTypeID.Value
					);
					Result.CurrentFundingTypeID = currentFundingType != null ? currentFundingType.FundingTypeID : 0;
				} // if
			} // if

			if (currentProductType != null) {
				var currentProduct = Result.Products.FirstOrDefault(x => x.ProductID == currentProductType.ProductID);
				currentProduct = currentProduct ?? Result.Products.FirstOrDefault(x => x.IsDefault);
				Result.CurrentProductID = currentProduct != null ? currentProduct.ProductID : 0;
			} // if
			
			Result.CurrentGradeRange = Result.GradeRanges.FirstOrDefault(x => 
				x.GradeID.HasValue && x.GradeID.Value == Result.GradeID &&
				x.SubGradeID == Result.SubGradeID &&
				x.LoanSourceID == Result.LoanSourceID &&
				x.OriginID == Result.OriginID &&
				x.IsFirstLoan == (Result.NumOfLoans == 0));
		} // BuildDefaultProduct

		private void ProcessRow(SafeReader sr) {
			RowTypes rowType;
			string rowTypeStr = sr["RowType"];

			if (!Enum.TryParse(rowTypeStr, false, out rowType)) {
				Log.Alert("Unknown row type '{0}' while loading application info model.", rowTypeStr);
				return;
			} // if

			switch (rowType) {
			case RowTypes.LoanSource:
				this.loanSources.Add(sr.Fill<LoanSourceModel>());
				break;

			case RowTypes.LoanType:
				this.loanTypes.Add(sr.Fill<LoanTypeModel>());
				break;

			case RowTypes.DiscountPlan:
				this.discountPlans.Add(sr.Fill<DiscountPlanModel>());
				break;

			case RowTypes.Model:
				sr.Fill(Result);
				this.rawOfferStart = sr["RawOfferStart"];
				this.brokerCardID = sr["BrokerCardID"];
				this.brokerID = sr["BrokerID"];
				string typeOfBusinessStr = sr["TypeOfBusiness"];

				TypeOfBusiness typeOfBusiness;
				if(Enum.TryParse(typeOfBusinessStr, out typeOfBusiness))
					Result.TypeOfBusiness = typeOfBusiness;

				break;

			case RowTypes.OfferCalculation:
				sr.Fill(Result.AutomationOfferModel);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		private void BuildSuggestedAmountModel() {
			var stra = new GetCurrentCustomerAnnualTurnover(Result.CustomerId);
			stra.Execute();
			Result.Turnover = stra.Turnover;

			Result.SuggestedAmounts = new[] {
				new SuggestedAmountModel {
					Method = CalculationMethod.Turnover.DescriptionAttr(),
					Silver = 0.06M,
					Gold = 0.08M,
					Platinum = 0.1M,
					Diamond = 0.12M,
					Value = Result.Turnover
				},
				new SuggestedAmountModel {
					Method = CalculationMethod.ValueAdded.DescriptionAttr(),
					Silver = 0.15M,
					Gold = 0.20M,
					Platinum = 0.25M,
					Diamond = 0.30M,
					Value = Result.ValueAdded
				},
				new SuggestedAmountModel {
					Method = CalculationMethod.FCF.DescriptionAttr(),
					Silver = 0.29M,
					Gold = 0.38M,
					Platinum = 0.48M,
					Diamond = 0.58M,
					Value = Result.FreeCashFlow
				},
			};
		} // BuildSuggestedAmountModel

		private void BuildSetupFee() {
			var fees = new SetupFeeCalculator(Result.ManualSetupFeePercent, Result.BrokerSetupFeePercent)
				.CalculateTotalAndBroker(Result.OfferedCreditLine);

			Result.TotalSetupFee = fees.Total;
			Result.BrokerSetupFee = fees.Broker;
		} // BuildSetupFee

		private void BuildLoanCost() {
			Result.Apr = 0;

			var loan = CreateNewLoan();

			if (loan.LoanAmount == 0)
				return;

			Result.Apr = new APRCalculator().Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date);

			Result.RealCost =
				(loan.Schedule.Sum(s => s.Interest) + loan.Charges.Sum(x => x.Amount) + loan.SetupFee) / loan.LoanAmount;
		} // BuildLoanCost

		private Loan CreateNewLoan() {
			var calculator = new LoanScheduleCalculator { Interest = Result.InterestRate, Term = Result.RepaymentPeriod, };

			CashRequest cr = CreateMinimalCashRequest();

			var loan = new Loan {
				LoanAmount = Result.OfferedCreditLine,
				Date = this.rawOfferStart ?? this.now,
				LoanType = cr.LoanType,
				CashRequest = cr,
				SetupFee = Result.TotalSetupFee,
				LoanLegalId = null,
			};

			calculator.Calculate(Result.OfferedCreditLine, loan, loan.Date, 0, Result.SpreadSetupFee);

			loan.LoanSource = cr.LoanSource;

			if ((Result.BrokerSetupFee > 0) && (this.brokerID != null)) {
				loan.BrokerCommissions.Add(new LoanBrokerCommission {
					Broker = ObjectFactory.GetInstance<BrokerRepository>().GetByID(this.brokerID.Value),
					CardInfo = this.brokerCardID == null
						? null
						: ObjectFactory.GetInstance<ICardInfoRepository>().GetAll().FirstOrDefault(ci =>
							ci.Id == this.brokerCardID.Value
						),
					CommissionAmount = Result.BrokerSetupFee,
					CreateDate = this.rawOfferStart ?? this.now,
					Loan = loan,
				});
			} // if broker fee & broker

			return loan;
		} // CreateNewLoan

		private CashRequest CreateMinimalCashRequest() {
			var loanTypeRepo = ObjectFactory.GetInstance<ILoanTypeRepository>();
			var loanSourceRepo = ObjectFactory.GetInstance<ILoanSourceRepository>();
			var discountPlanRepo = ObjectFactory.GetInstance<IDiscountPlanRepository>();

			return new CashRequest {
				LoanType = loanTypeRepo.GetAll().FirstOrDefault(lt => lt.Id == Result.LoanTypeId)
					?? loanTypeRepo.GetDefault(),
				LoanSource = loanSourceRepo.GetAll().FirstOrDefault(ls => ls.ID == Result.LoanSourceID)
					?? loanSourceRepo.GetDefault(this.customerID),
				DiscountPlan = discountPlanRepo.GetAll().FirstOrDefault(dp => dp.Id == Result.DiscountPlanId)
					?? discountPlanRepo.GetDefault(),
			};
		} // CreateMinimalCashRequest

		private enum RowTypes {
			LoanSource,
			LoanType,
			DiscountPlan,
			Model,
			OfferCalculation,
		} // enum RowTypes

		private DateTime? rawOfferStart;
		private int? brokerID;
		private int? brokerCardID;
		private readonly int customerID;
		private readonly long? cashRequestID;
		private readonly DateTime now;
		private readonly List<LoanTypeModel> loanTypes;
		private readonly List<DiscountPlanModel> discountPlans;
		private readonly List<LoanSourceModel> loanSources;
	} // class LoadApplicationInfo
} // namespace

