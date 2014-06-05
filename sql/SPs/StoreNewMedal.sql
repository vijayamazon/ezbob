IF OBJECT_ID('StoreNewMedal') IS NULL
	EXECUTE('CREATE PROCEDURE StoreNewMedal AS SELECT 1')
GO

ALTER PROCEDURE StoreNewMedal
	(@CustomerId INT
	,@BusinessScoreWeight DECIMAL(18,6)
	,@BusinessScoreGrade DECIMAL(18,6)
	,@BusinessScoreScore DECIMAL(18,6)
	,@FreeCashFlowWeight DECIMAL(18,6)
	,@FreeCashFlowGrade DECIMAL(18,6)
	,@FreeCashFlowScore DECIMAL(18,6)
	,@AnnualTurnoverWeight DECIMAL(18,6)
	,@AnnualTurnoverGrade DECIMAL(18,6)
	,@AnnualTurnoverScore DECIMAL(18,6)
	,@TangibleEquityWeight DECIMAL(18,6)
	,@TangibleEquityGrade DECIMAL(18,6)
	,@TangibleEquityScore DECIMAL(18,6)
	,@BusinessSeniorityWeight DECIMAL(18,6)
	,@BusinessSeniorityGrade DECIMAL(18,6)
	,@BusinessSeniorityScore DECIMAL(18,6)
	,@ConsumerScoreWeight DECIMAL(18,6)
	,@ConsumerScoreGrade DECIMAL(18,6)
	,@ConsumerScoreScore DECIMAL(18,6)
	,@NetWorthWeight DECIMAL(18,6)
	,@NetWorthGrade DECIMAL(18,6)
	,@NetWorthScore DECIMAL(18,6)
	,@MaritalStatusWeight DECIMAL(18,6)
	,@MaritalStatusGrade DECIMAL(18,6)
	,@MaritalStatusScore DECIMAL(18,6)
	,@EzbobSeniorityWeight DECIMAL(18,6)
	,@EzbobSeniorityGrade DECIMAL(18,6)
	,@EzbobSeniorityScore DECIMAL(18,6)
	,@NumOfLoansWeight DECIMAL(18,6)
	,@NumOfLoansGrade DECIMAL(18,6)
	,@NumOfLoansScore DECIMAL(18,6)
	,@NumOfLateRepaymentsWeight DECIMAL(18,6)
	,@NumOfLateRepaymentsGrade DECIMAL(18,6)
	,@NumOfLateRepaymentsScore DECIMAL(18,6)
	,@NumOfEarlyReaymentsWeight DECIMAL(18,6)
	,@NumOfEarlyReaymentsGrade DECIMAL(18,6)
	,@NumOfEarlyReaymentsScore DECIMAL(18,6)
	,@TotalScore DECIMAL(18,6)
	,@TotalScoreNormalized DECIMAL(18,6)
	,@Medal NVARCHAR(50))
AS
BEGIN
	INSERT INTO NewScoreStorage (
	 CustomerId
	,BusinessScoreWeight
	,BusinessScoreGrade
	,BusinessScoreScore
	,FreeCashFlowWeight
	,FreeCashFlowGrade
	,FreeCashFlowScore
	,AnnualTurnoverWeight
	,AnnualTurnoverGrade
	,AnnualTurnoverScore
	,TangibleEquityWeight
	,TangibleEquityGrade
	,TangibleEquityScore
	,BusinessSeniorityWeight
	,BusinessSeniorityGrade
	,BusinessSeniorityScore
	,ConsumerScoreWeight
	,ConsumerScoreGrade
	,ConsumerScoreScore
	,NetWorthWeight
	,NetWorthGrade
	,NetWorthScore
	,MaritalStatusWeight
	,MaritalStatusGrade
	,MaritalStatusScore
	,EzbobSeniorityWeight
	,EzbobSeniorityGrade
	,EzbobSeniorityScore
	,NumOfLoansWeight
	,NumOfLoansGrade
	,NumOfLoansScore
	,NumOfLateRepaymentsWeight
	,NumOfLateRepaymentsGrade
	,NumOfLateRepaymentsScore
	,NumOfEarlyReaymentsWeight
	,NumOfEarlyReaymentsGrade
	,NumOfEarlyReaymentsScore
	,TotalScore
	,TotalScoreNormalized
	,Medal)
	VALUES (
	 @BusinessScoreWeight
	,@CustomerId
	,@BusinessScoreGrade
	,@BusinessScoreScore
	,@FreeCashFlowWeight
	,@FreeCashFlowGrade
	,@FreeCashFlowScore
	,@AnnualTurnoverWeight
	,@AnnualTurnoverGrade
	,@AnnualTurnoverScore
	,@TangibleEquityWeight
	,@TangibleEquityGrade
	,@TangibleEquityScore
	,@BusinessSeniorityWeight
	,@BusinessSeniorityGrade
	,@BusinessSeniorityScore
	,@ConsumerScoreWeight
	,@ConsumerScoreGrade
	,@ConsumerScoreScore
	,@NetWorthWeight
	,@NetWorthGrade
	,@NetWorthScore
	,@MaritalStatusWeight
	,@MaritalStatusGrade
	,@MaritalStatusScore
	,@EzbobSeniorityWeight
	,@EzbobSeniorityGrade
	,@EzbobSeniorityScore
	,@NumOfLoansWeight
	,@NumOfLoansGrade
	,@NumOfLoansScore
	,@NumOfLateRepaymentsWeight
	,@NumOfLateRepaymentsGrade
	,@NumOfLateRepaymentsScore
	,@NumOfEarlyReaymentsWeight
	,@NumOfEarlyReaymentsGrade
	,@NumOfEarlyReaymentsScore
	,@TotalScore
	,@TotalScoreNormalized
	,@Medal)
END
GO
