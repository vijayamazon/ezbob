IF OBJECT_ID('StoreNewMedal') IS NULL
	EXECUTE('CREATE PROCEDURE StoreNewMedal AS SELECT 1')
GO

ALTER PROCEDURE StoreNewMedal
	(@CustomerId INT
	,@MedalType NVARCHAR(20)
	,@BusinessScore INT
	,@BusinessScoreWeight DECIMAL(18,6)
	,@BusinessScoreGrade DECIMAL(18,6)
	,@BusinessScoreScore DECIMAL(18,6)
	,@FreeCashFlowValue DECIMAL(18,6)
	,@FreeCashFlow DECIMAL(18,6)
	,@FreeCashFlowWeight DECIMAL(18,6)
	,@FreeCashFlowGrade DECIMAL(18,6)
	,@FreeCashFlowScore DECIMAL(18,6)
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
	,@Error NVARCHAR(500))
AS
BEGIN
	UPDATE MedalCalculations SET IsActive = 0 WHERE IsActive = 1 AND CustomerId = @CustomerId

	INSERT INTO MedalCalculations (
	 CustomerId
	,MedalType
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
	,Error)
	VALUES (
	 @CustomerId
	,@MedalType
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
	,@Error)
END
GO
