IF(object_id('MedalCalculationsAV') IS NULL) 
BEGIN

CREATE TABLE MedalCalculationsAV
(
	 Id INT IDENTITY
	,CustomerId INT NOT NULL
	,IsActive BIT NOT NULL DEFAULT(0)
	,CalculationTime DATETIME NOT NULL
	,Error NVARCHAR (500)
	,TotalScore DECIMAL(18,6)
	,TotalScoreNormalized DECIMAL(18,6)
	,Medal NVARCHAR(50)	
	,MedalType NVARCHAR(20)
	
	,BusinessScore INT
	,BusinessScoreWeight DECIMAL(18,6)
	,BusinessScoreGrade INT
	,BusinessScoreScore DECIMAL(18,6)
	,FreeCashFlow DECIMAL(18,6)
	,FreeCashFlowWeight DECIMAL(18,6)
	,FreeCashFlowGrade INT
	,FreeCashFlowScore DECIMAL(18,6)
	,AnnualTurnover DECIMAL(18,6)
	,AnnualTurnoverWeight DECIMAL(18,6)
	,AnnualTurnoverGrade INT
	,AnnualTurnoverScore DECIMAL(18,6)
	,TangibleEquity DECIMAL(18,6)
	,TangibleEquityWeight DECIMAL(18,6)
	,TangibleEquityGrade INT
	,TangibleEquityScore DECIMAL(18,6)
	,BusinessSeniority DECIMAL(18,6)
	,BusinessSeniorityWeight DECIMAL(18,6)
	,BusinessSeniorityGrade INT
	,BusinessSeniorityScore DECIMAL(18,6)
	,ConsumerScore INT
	,ConsumerScoreWeight DECIMAL(18,6)
	,ConsumerScoreGrade INT
	,ConsumerScoreScore DECIMAL(18,6)
	,NetWorth DECIMAL(18,6)
	,NetWorthWeight DECIMAL(18,6)
	,NetWorthGrade INT
	,NetWorthScore DECIMAL(18,6)
	,MaritalStatus NVARCHAR(50)
	,MaritalStatusWeight DECIMAL(18,6)
	,MaritalStatusGrade INT
	,MaritalStatusScore DECIMAL(18,6)	
	,NumberOfStores INT
	,NumberOfStoresWeight DECIMAL(18,6)
	,NumberOfStoresGrade INT
	,NumberOfStoresScore DECIMAL(18,6)	
	,PositiveFeedbacks INT
	,PositiveFeedbacksWeight DECIMAL(18,6)
	,PositiveFeedbacksGrade INT
	,PositiveFeedbacksScore DECIMAL(18,6)	
	,EzbobSeniority DECIMAL(18,6)
	,EzbobSeniorityWeight DECIMAL(18,6)
	,EzbobSeniorityGrade INT
	,EzbobSeniorityScore DECIMAL(18,6)
	,NumOfLoans INT
	,NumOfLoansWeight DECIMAL(18,6)
	,NumOfLoansGrade INT
	,NumOfLoansScore DECIMAL(18,6)
	,NumOfLateRepayments INT
	,NumOfLateRepaymentsWeight DECIMAL(18,6)
	,NumOfLateRepaymentsGrade INT
	,NumOfLateRepaymentsScore DECIMAL(18,6)
	,NumOfEarlyRepayments INT
	,NumOfEarlyRepaymentsWeight DECIMAL(18,6)
	,NumOfEarlyRepaymentsGrade INT
	,NumOfEarlyRepaymentsScore DECIMAL(18,6)
	,ValueAdded DECIMAL(18,6)
	,OfferedLoanAmount INT
	,NumOfHmrcMps INT
	,FirstRepaymentDatePassed BIT
	,AmazonPositiveFeedbacks INT
	,EbayPositiveFeedbacks INT
	,NumberOfPaypalPositiveTransactions INT
	,CONSTRAINT PK_MedalCalculationsAV PRIMARY KEY (Id)
)

END 
GO

