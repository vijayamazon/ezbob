IF(object_id('NewMedalComparison1') IS NULL) 
BEGIN

CREATE TABLE NewMedalComparison1
(
	Id INT IDENTITY
	,CustomerId INT NOT NULL
	,BusinessScore INT
	,BusinessScoreWeight DECIMAL(18,6)
	,BusinessScoreGrade DECIMAL(18,6)
	,BusinessScoreScore DECIMAL(18,6)
	,FreeCashFlow DECIMAL(18,6)
	,FreeCashFlowWeight DECIMAL(18,6)
	,FreeCashFlowGrade DECIMAL(18,6)
	,FreeCashFlowScore DECIMAL(18,6)
	,AnnualTurnover DECIMAL(18,6)
	,AnnualTurnoverWeight DECIMAL(18,6)
	,AnnualTurnoverGrade DECIMAL(18,6)
	,AnnualTurnoverScore DECIMAL(18,6)
	,TangibleEquity DECIMAL(18,6)
	,TangibleEquityWeight DECIMAL(18,6)
	,TangibleEquityGrade DECIMAL(18,6)
	,TangibleEquityScore DECIMAL(18,6)
	,BusinessSeniority DATETIME
	,BusinessSeniorityWeight DECIMAL(18,6)
	,BusinessSeniorityGrade DECIMAL(18,6)
	,BusinessSeniorityScore DECIMAL(18,6)
	,ConsumerScore INT
	,ConsumerScoreWeight DECIMAL(18,6)
	,ConsumerScoreGrade DECIMAL(18,6)
	,ConsumerScoreScore DECIMAL(18,6)
	,NetWorth DECIMAL(18,6)
	,NetWorthWeight DECIMAL(18,6)
	,NetWorthGrade DECIMAL(18,6)
	,NetWorthScore DECIMAL(18,6)
	,MaritalStatus NVARCHAR(50)
	,MaritalStatusWeight DECIMAL(18,6)
	,MaritalStatusGrade DECIMAL(18,6)
	,MaritalStatusScore DECIMAL(18,6)
	,EzbobSeniority DATETIME
	,EzbobSeniorityWeight DECIMAL(18,6)
	,EzbobSeniorityGrade DECIMAL(18,6)
	,EzbobSeniorityScore DECIMAL(18,6)
	,NumOfLoans INT
	,NumOfLoansWeight DECIMAL(18,6)
	,NumOfLoansGrade DECIMAL(18,6)
	,NumOfLoansScore DECIMAL(18,6)
	,NumOfLateRepayments INT
	,NumOfLateRepaymentsWeight DECIMAL(18,6)
	,NumOfLateRepaymentsGrade DECIMAL(18,6)
	,NumOfLateRepaymentsScore DECIMAL(18,6)
	,NumOfEarlyRepayments INT
	,NumOfEarlyRepaymentsWeight DECIMAL(18,6)
	,NumOfEarlyRepaymentsGrade DECIMAL(18,6)
	,NumOfEarlyRepaymentsScore DECIMAL(18,6)
	,TotalScore DECIMAL(18,6)
	,TotalScoreNormalized DECIMAL(18,6)
	,Medal NVARCHAR(50)
	,Error NVARCHAR(500)	
	,CONSTRAINT PK_NewMedalComparison1 PRIMARY KEY (Id)
)

END 
GO

IF(object_id('NewMedalComparison2') IS NULL) 
BEGIN

CREATE TABLE NewMedalComparison2
(
	Id INT IDENTITY
	,CustomerId INT NOT NULL
	,BusinessScore INT
	,BusinessScoreWeight DECIMAL(18,6)
	,BusinessScoreGrade DECIMAL(18,6)
	,BusinessScoreScore DECIMAL(18,6)
	,FreeCashFlow DECIMAL(18,6)
	,FreeCashFlowWeight DECIMAL(18,6)
	,FreeCashFlowGrade DECIMAL(18,6)
	,FreeCashFlowScore DECIMAL(18,6)
	,AnnualTurnover DECIMAL(18,6)
	,AnnualTurnoverWeight DECIMAL(18,6)
	,AnnualTurnoverGrade DECIMAL(18,6)
	,AnnualTurnoverScore DECIMAL(18,6)
	,TangibleEquity DECIMAL(18,6)
	,TangibleEquityWeight DECIMAL(18,6)
	,TangibleEquityGrade DECIMAL(18,6)
	,TangibleEquityScore DECIMAL(18,6)
	,BusinessSeniority DATETIME
	,BusinessSeniorityWeight DECIMAL(18,6)
	,BusinessSeniorityGrade DECIMAL(18,6)
	,BusinessSeniorityScore DECIMAL(18,6)
	,ConsumerScore INT
	,ConsumerScoreWeight DECIMAL(18,6)
	,ConsumerScoreGrade DECIMAL(18,6)
	,ConsumerScoreScore DECIMAL(18,6)
	,NetWorth DECIMAL(18,6)
	,NetWorthWeight DECIMAL(18,6)
	,NetWorthGrade DECIMAL(18,6)
	,NetWorthScore DECIMAL(18,6)
	,MaritalStatus NVARCHAR(50)
	,MaritalStatusWeight DECIMAL(18,6)
	,MaritalStatusGrade DECIMAL(18,6)
	,MaritalStatusScore DECIMAL(18,6)
	,EzbobSeniority DATETIME
	,EzbobSeniorityWeight DECIMAL(18,6)
	,EzbobSeniorityGrade DECIMAL(18,6)
	,EzbobSeniorityScore DECIMAL(18,6)
	,NumOfLoans INT
	,NumOfLoansWeight DECIMAL(18,6)
	,NumOfLoansGrade DECIMAL(18,6)
	,NumOfLoansScore DECIMAL(18,6)
	,NumOfLateRepayments INT
	,NumOfLateRepaymentsWeight DECIMAL(18,6)
	,NumOfLateRepaymentsGrade DECIMAL(18,6)
	,NumOfLateRepaymentsScore DECIMAL(18,6)
	,NumOfEarlyRepayments INT
	,NumOfEarlyRepaymentsWeight DECIMAL(18,6)
	,NumOfEarlyRepaymentsGrade DECIMAL(18,6)
	,NumOfEarlyRepaymentsScore DECIMAL(18,6)
	,TotalScore DECIMAL(18,6)
	,TotalScoreNormalized DECIMAL(18,6)
	,Medal NVARCHAR(50)
	,Error NVARCHAR(500)	
	,CONSTRAINT PK_NewMedalComparison2 PRIMARY KEY (Id)
)

END 
GO
