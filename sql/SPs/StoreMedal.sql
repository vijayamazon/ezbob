SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('StoreMedal') IS NULL
	EXECUTE('CREATE PROCEDURE StoreMedal AS SELECT 1')
GO

ALTER PROCEDURE StoreMedal
	 @CustomerId INT
	,@CalculationTime DATETIME
	,@MedalType NVARCHAR(50)
	,@FirstRepaymentDatePassed BIT
	,@BusinessScore INT
	,@BusinessScoreWeight DECIMAL(18,6)
	,@BusinessScoreGrade DECIMAL(18,6)
	,@BusinessScoreScore DECIMAL(18,6)
	,@FreeCashFlowValue DECIMAL(18,6)
	,@FreeCashFlow DECIMAL(18,6)
	,@FreeCashFlowWeight DECIMAL(18,6)
	,@FreeCashFlowGrade DECIMAL(18,6)
	,@FreeCashFlowScore DECIMAL(18,6)
	,@HmrcAnnualTurnover DECIMAL(18,6)
	,@BankAnnualTurnover DECIMAL(18,6)
	,@OnlineAnnualTurnover DECIMAL(18,6)
	,@AnnualTurnover DECIMAL(18,6)
	,@AnnualTurnoverWeight DECIMAL(18,6)
	,@AnnualTurnoverGrade DECIMAL(18,6)
	,@AnnualTurnoverScore DECIMAL(18,6)
	,@TangibleEquityValue DECIMAL(18,6)
	,@TangibleEquity DECIMAL(18,6)
	,@TangibleEquityWeight DECIMAL(18,6)
	,@TangibleEquityGrade DECIMAL(18,6)
	,@TangibleEquityScore DECIMAL(18,6)
	,@BusinessSeniority DATETIME
	,@BusinessSeniorityWeight DECIMAL(18,6)
	,@BusinessSeniorityGrade DECIMAL(18,6)
	,@BusinessSeniorityScore DECIMAL(18,6)
	,@ConsumerScore INT
	,@ConsumerScoreWeight DECIMAL(18,6)
	,@ConsumerScoreGrade DECIMAL(18,6)
	,@ConsumerScoreScore DECIMAL(18,6)
	,@NetWorth DECIMAL(18,6)
	,@NetWorthWeight DECIMAL(18,6)
	,@NetWorthGrade DECIMAL(18,6)
	,@NetWorthScore DECIMAL(18,6)
	,@MaritalStatus NVARCHAR(50)
	,@MaritalStatusWeight DECIMAL(18,6)
	,@MaritalStatusGrade DECIMAL(18,6)
	,@MaritalStatusScore DECIMAL(18,6)
	,@NumberOfStores INT
	,@NumberOfStoresWeight DECIMAL(18,6)
	,@NumberOfStoresGrade DECIMAL(18,6)
	,@NumberOfStoresScore DECIMAL(18,6)
	,@PositiveFeedbacks INT
	,@PositiveFeedbacksWeight DECIMAL(18,6)
	,@PositiveFeedbacksGrade DECIMAL(18,6)
	,@PositiveFeedbacksScore DECIMAL(18,6)
	,@EzbobSeniority DATETIME
	,@EzbobSeniorityWeight DECIMAL(18,6)
	,@EzbobSeniorityGrade DECIMAL(18,6)
	,@EzbobSeniorityScore DECIMAL(18,6)
	,@NumOfLoans INT
	,@NumOfLoansWeight DECIMAL(18,6)
	,@NumOfLoansGrade DECIMAL(18,6)
	,@NumOfLoansScore DECIMAL(18,6)
	,@NumOfLateRepayments INT
	,@NumOfLateRepaymentsWeight DECIMAL(18,6)
	,@NumOfLateRepaymentsGrade DECIMAL(18,6)
	,@NumOfLateRepaymentsScore DECIMAL(18,6)
	,@NumOfEarlyRepayments INT
	,@NumOfEarlyRepaymentsWeight DECIMAL(18,6)
	,@NumOfEarlyRepaymentsGrade DECIMAL(18,6)
	,@NumOfEarlyRepaymentsScore DECIMAL(18,6)
	,@ValueAdded DECIMAL(18,6)
	,@InnerFlowName NVARCHAR(20)
	,@TotalScore DECIMAL(18,6)
	,@TotalScoreNormalized DECIMAL(18,6)
	,@Medal NVARCHAR(50)
	,@Error NVARCHAR(500)
	,@OfferedLoanAmount INT
	,@NumOfHmrcMps INT
	,@ZooplaValue INT
	,@EarliestHmrcLastUpdateDate DATETIME
	,@EarliestYodleeLastUpdateDate DATETIME
	,@AmazonPositiveFeedbacks INT
	,@EbayPositiveFeedbacks INT
	,@NumberOfPaypalPositiveTransactions INT
	,@MortgageBalance DECIMAL(18,6)
	,@CapOfferByCustomerScoresValue DECIMAL(18,6)
	,@CapOfferByCustomerScoresTable NVARCHAR(MAX)
	,@Tag NVARCHAR(256)
	,@MaxOfferedLoanAmount INT
	,@CashRequestID BIGINT 
	,@NLCashRequestID BIGINT = NULL
AS
BEGIN
	SET QUOTED_IDENTIFIER ON;

	------------------------------------------------------------------------------

	DECLARE @TagID BIGINT = NULL

	EXECUTE SaveOrGetDecisionTrailTag @Tag, @TagID OUTPUT

	------------------------------------------------------------------------------

	UPDATE MedalCalculations SET IsActive = 0 WHERE IsActive = 1 AND CustomerId = @CustomerId

	------------------------------------------------------------------------------

	INSERT INTO MedalCalculations (
		 CustomerId
		,CalculationTime
		,MedalType
		,FirstRepaymentDatePassed
		,IsActive
		,BusinessScore
		,BusinessScoreWeight
		,BusinessScoreGrade
		,BusinessScoreScore
		,FreeCashFlowValue
		,FreeCashFlow
		,FreeCashFlowWeight
		,FreeCashFlowGrade
		,FreeCashFlowScore
		,HmrcAnnualTurnover
		,BankAnnualTurnover
		,OnlineAnnualTurnover
		,AnnualTurnover
		,AnnualTurnoverWeight
		,AnnualTurnoverGrade
		,AnnualTurnoverScore
		,TangibleEquityValue
		,TangibleEquity
		,TangibleEquityWeight
		,TangibleEquityGrade
		,TangibleEquityScore
		,BusinessSeniority
		,BusinessSeniorityWeight
		,BusinessSeniorityGrade
		,BusinessSeniorityScore
		,ConsumerScore
		,ConsumerScoreWeight
		,ConsumerScoreGrade
		,ConsumerScoreScore
		,NetWorth
		,NetWorthWeight
		,NetWorthGrade
		,NetWorthScore
		,MaritalStatus
		,MaritalStatusWeight
		,MaritalStatusGrade
		,MaritalStatusScore
		,NumberOfStores
		,NumberOfStoresWeight
		,NumberOfStoresGrade
		,NumberOfStoresScore
		,PositiveFeedbacks
		,PositiveFeedbacksWeight
		,PositiveFeedbacksGrade
		,PositiveFeedbacksScore
		,EzbobSeniority
		,EzbobSeniorityWeight
		,EzbobSeniorityGrade
		,EzbobSeniorityScore
		,NumOfLoans
		,NumOfLoansWeight
		,NumOfLoansGrade
		,NumOfLoansScore
		,NumOfLateRepayments
		,NumOfLateRepaymentsWeight
		,NumOfLateRepaymentsGrade
		,NumOfLateRepaymentsScore
		,NumOfEarlyRepayments
		,NumOfEarlyRepaymentsWeight
		,NumOfEarlyRepaymentsGrade
		,NumOfEarlyRepaymentsScore
		,ValueAdded
		,InnerFlowName
		,TotalScore
		,TotalScoreNormalized
		,Medal
		,Error
		,OfferedLoanAmount
		,NumOfHmrcMps
		,ZooplaValue	
		,EarliestHmrcLastUpdateDate
		,EarliestYodleeLastUpdateDate
		,AmazonPositiveFeedbacks
		,EbayPositiveFeedbacks
		,NumberOfPaypalPositiveTransactions
		,MortgageBalance
		,CapOfferByCustomerScoresValue
		,CapOfferByCustomerScoresTable
		,TrailTagID
		,MaxOfferedLoanAmount
		,CashRequestID
		,NLCashRequestID
	) VALUES (
		 @CustomerId
		,@CalculationTime
		,@MedalType
		,@FirstRepaymentDatePassed
		,1
		,@BusinessScore
		,@BusinessScoreWeight
		,@BusinessScoreGrade
		,@BusinessScoreScore
		,@FreeCashFlowValue
		,@FreeCashFlow
		,@FreeCashFlowWeight
		,@FreeCashFlowGrade
		,@FreeCashFlowScore
		,@HmrcAnnualTurnover
		,@BankAnnualTurnover
		,@OnlineAnnualTurnover
		,@AnnualTurnover
		,@AnnualTurnoverWeight
		,@AnnualTurnoverGrade
		,@AnnualTurnoverScore
		,@TangibleEquityValue
		,@TangibleEquity
		,@TangibleEquityWeight
		,@TangibleEquityGrade
		,@TangibleEquityScore
		,@BusinessSeniority
		,@BusinessSeniorityWeight
		,@BusinessSeniorityGrade
		,@BusinessSeniorityScore
		,@ConsumerScore
		,@ConsumerScoreWeight
		,@ConsumerScoreGrade
		,@ConsumerScoreScore
		,@NetWorth
		,@NetWorthWeight
		,@NetWorthGrade
		,@NetWorthScore
		,@MaritalStatus
		,@MaritalStatusWeight
		,@MaritalStatusGrade
		,@MaritalStatusScore
		,@NumberOfStores
		,@NumberOfStoresWeight
		,@NumberOfStoresGrade
		,@NumberOfStoresScore
		,@PositiveFeedbacks
		,@PositiveFeedbacksWeight
		,@PositiveFeedbacksGrade
		,@PositiveFeedbacksScore
		,@EzbobSeniority
		,@EzbobSeniorityWeight
		,@EzbobSeniorityGrade
		,@EzbobSeniorityScore
		,@NumOfLoans
		,@NumOfLoansWeight
		,@NumOfLoansGrade
		,@NumOfLoansScore
		,@NumOfLateRepayments
		,@NumOfLateRepaymentsWeight
		,@NumOfLateRepaymentsGrade
		,@NumOfLateRepaymentsScore
		,@NumOfEarlyRepayments
		,@NumOfEarlyRepaymentsWeight
		,@NumOfEarlyRepaymentsGrade
		,@NumOfEarlyRepaymentsScore
		,@ValueAdded
		,@InnerFlowName
		,@TotalScore
		,@TotalScoreNormalized
		,@Medal
		,@Error
		,@OfferedLoanAmount
		,@NumOfHmrcMps
		,@ZooplaValue
		,@EarliestHmrcLastUpdateDate
		,@EarliestYodleeLastUpdateDate
		,@AmazonPositiveFeedbacks
		,@EbayPositiveFeedbacks
		,@NumberOfPaypalPositiveTransactions
		,@MortgageBalance
		,@CapOfferByCustomerScoresValue
		,@CapOfferByCustomerScoresTable
		,@TagID
		,@MaxOfferedLoanAmount
		,@CashRequestID
		,@NLCashRequestID
	)
END
GO
