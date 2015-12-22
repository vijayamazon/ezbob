SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_StoreNewMedal') IS NULL
	EXECUTE('CREATE PROCEDURE AV_StoreNewMedal AS SELECT 1')
GO

ALTER PROCEDURE AV_StoreNewMedal
(@CustomerId INT
,@CalculationTime DATETIME
,@MedalType NVARCHAR(50)
,@FirstRepaymentDatePassed BIT
,@BusinessScore INT
,@BusinessScoreWeight DECIMAL(18,6)
,@BusinessScoreGrade INT
,@BusinessScoreScore DECIMAL(18,6)
,@FreeCashFlow DECIMAL(18,6)
,@FreeCashFlowWeight DECIMAL(18,6)
,@FreeCashFlowGrade INT
,@FreeCashFlowScore DECIMAL(18,6)
,@AnnualTurnover DECIMAL(18,6)
,@AnnualTurnoverWeight DECIMAL(18,6)
,@AnnualTurnoverGrade INT
,@AnnualTurnoverScore DECIMAL(18,6)
,@TangibleEquity DECIMAL(18,6)
,@TangibleEquityWeight DECIMAL(18,6)
,@TangibleEquityGrade INT
,@TangibleEquityScore DECIMAL(18,6)
,@BusinessSeniority DECIMAL(18,6)
,@BusinessSeniorityWeight DECIMAL(18,6)
,@BusinessSeniorityGrade INT
,@BusinessSeniorityScore DECIMAL(18,6)
,@ConsumerScore INT
,@ConsumerScoreWeight DECIMAL(18,6)
,@ConsumerScoreGrade INT
,@ConsumerScoreScore DECIMAL(18,6)
,@NetWorth DECIMAL(18,6)
,@NetWorthWeight DECIMAL(18,6)
,@NetWorthGrade INT
,@NetWorthScore DECIMAL(18,6)
,@MaritalStatus NVARCHAR(50)
,@MaritalStatusWeight DECIMAL(18,6)
,@MaritalStatusGrade INT
,@MaritalStatusScore DECIMAL(18,6)	
,@NumberOfStores INT
,@NumberOfStoresWeight DECIMAL(18,6)
,@NumberOfStoresGrade INT
,@NumberOfStoresScore DECIMAL(18,6)	
,@PositiveFeedbacks INT
,@PositiveFeedbacksWeight DECIMAL(18,6)
,@PositiveFeedbacksGrade INT
,@PositiveFeedbacksScore DECIMAL(18,6)	
,@EzbobSeniority DECIMAL(18,6)
,@EzbobSeniorityWeight DECIMAL(18,6)
,@EzbobSeniorityGrade INT
,@EzbobSeniorityScore DECIMAL(18,6)
,@NumOfLoans INT
,@NumOfLoansWeight DECIMAL(18,6)
,@NumOfLoansGrade INT
,@NumOfLoansScore DECIMAL(18,6)
,@NumOfLateRepayments INT
,@NumOfLateRepaymentsWeight DECIMAL(18,6)
,@NumOfLateRepaymentsGrade INT
,@NumOfLateRepaymentsScore DECIMAL(18,6)
,@NumOfEarlyRepayments INT
,@NumOfEarlyRepaymentsWeight DECIMAL(18,6)
,@NumOfEarlyRepaymentsGrade INT
,@NumOfEarlyRepaymentsScore DECIMAL(18,6)
,@ValueAdded DECIMAL(18,6)
,@TotalScore DECIMAL(18,6)
,@TotalScoreNormalized DECIMAL(18,6)
,@Medal NVARCHAR(50)
,@Error NVARCHAR(500)
,@OfferedLoanAmount INT
,@NumOfHmrcMps INT
,@AmazonPositiveFeedbacks INT
,@EbayPositiveFeedbacks INT
,@NumberOfPaypalPositiveTransactions INT
,@CapOfferByCustomerScoresValue DECIMAL(18, 6)
,@CapOfferByCustomerScoresTable NVARCHAR(MAX)
,@Tag NVARCHAR(256)
,@MaxOfferedLoanAmount INT
,@CashRequestID BIGINT
,@NLCashRequestID BIGINT =NULL
)
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @TagID BIGINT = NULL

	EXECUTE SaveOrGetDecisionTrailTag @Tag, @TagID OUTPUT

	------------------------------------------------------------------------------

	UPDATE MedalCalculationsAV SET IsActive = 0 WHERE IsActive = 1 AND CustomerId = @CustomerId

	------------------------------------------------------------------------------

	INSERT INTO MedalCalculationsAV
		(CustomerId
		,CalculationTime
		,MedalType
		,FirstRepaymentDatePassed
		,IsActive
		,BusinessScore
		,BusinessScoreWeight
		,BusinessScoreGrade
		,BusinessScoreScore
		,FreeCashFlow
		,FreeCashFlowWeight
		,FreeCashFlowGrade
		,FreeCashFlowScore
		,AnnualTurnover
		,AnnualTurnoverWeight
		,AnnualTurnoverGrade
		,AnnualTurnoverScore
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
		,TotalScore
		,TotalScoreNormalized
		,Medal
		,Error
		,OfferedLoanAmount
		,NumOfHmrcMps
		,AmazonPositiveFeedbacks
		,EbayPositiveFeedbacks
		,NumberOfPaypalPositiveTransactions
		,CapOfferByCustomerScoresValue
		,CapOfferByCustomerScoresTable
		,TrailTagID
		,MaxOfferedLoanAmount
		,CashRequestID
		,NLCashRequestID 
	) VALUES
		(@CustomerId
		,@CalculationTime
		,@MedalType
		,@FirstRepaymentDatePassed
		,1
		,@BusinessScore
		,@BusinessScoreWeight
		,@BusinessScoreGrade
		,@BusinessScoreScore
		,@FreeCashFlow
		,@FreeCashFlowWeight
		,@FreeCashFlowGrade
		,@FreeCashFlowScore
		,@AnnualTurnover
		,@AnnualTurnoverWeight
		,@AnnualTurnoverGrade
		,@AnnualTurnoverScore
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
		,@TotalScore
		,@TotalScoreNormalized
		,@Medal
		,@Error
		,@OfferedLoanAmount
		,@NumOfHmrcMps
		,@AmazonPositiveFeedbacks
		,@EbayPositiveFeedbacks
		,@NumberOfPaypalPositiveTransactions
		,@CapOfferByCustomerScoresValue
		,@CapOfferByCustomerScoresTable
		,@TagID
		,@MaxOfferedLoanAmount
		,@CashRequestID
		,@NLCashRequestID 
	)
END
GO
