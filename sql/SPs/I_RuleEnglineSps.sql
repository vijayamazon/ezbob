SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_GetInvestorLoanCashRequest') IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetInvestorLoanCashRequest AS SELECT 1')
GO
IF OBJECT_ID('I_GetInvestorsBalance')IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetInvestorsBalance AS SELECT 1')
GO
IF OBJECT_ID('I_GetInvestorsIds') IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetInvestorsIds AS SELECT 1')
GO
IF OBJECT_ID('I_GetInvestorParametersDB')IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetInvestorParametersDB AS SELECT 1')
GO
IF OBJECT_ID('I_GetGradeMonthlyInvestedAmount') IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetGradeMonthlyInvestedAmount AS SELECT 1')
GO
IF OBJECT_ID('I_IndexLoad') IS NULL 
	EXECUTE('CREATE PROCEDURE I_IndexLoad AS SELECT 1')
GO
IF OBJECT_ID('I_GetInvestorTotalMonthlyDeposits') IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetInvestorTotalMonthlyDeposits AS SELECT 1')
GO
IF OBJECT_ID('I_GetInvestorBalanceMonthAgo') IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetInvestorBalanceMonthAgo AS SELECT 1')
GO
IF OBJECT_ID('I_GetInvestorMonthlyFundingCapital') IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetInvestorMonthlyFundingCapital AS SELECT 1')
GO
IF OBJECT_ID('I_GetFundedAmountPeriod') IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetFundedAmountPeriod AS SELECT 1')
GO
IF OBJECT_ID('I_GetInvestorRules') IS NULL
	EXECUTE('CREATE PROCEDURE I_GetInvestorRules AS SELECT 1')
GO

IF OBJECT_ID('I_GetInvestorWithLatestLoanDate') IS NULL
	EXECUTE('CREATE PROCEDURE I_GetInvestorWithLatestLoanDate AS SELECT 1')
GO

IF OBJECT_ID('I_GetInvestorParameters') IS NULL
	EXECUTE('CREATE PROCEDURE I_GetInvestorParameters AS SELECT 1')
GO


ALTER PROCEDURE I_GetInvestorRules
@RuleType INT,
@InvestorId INT = NULL
AS
BEGIN
	SELECT 
	RuleID,
	UserID,
	RuleType,
	InvestorID,
	FuncName,
	MemberNameSource,
	MemberNameTarget,
	LeftParamID,
	RightParamID,
	Operator,
	IsRoot 
	FROM
		I_InvestorRule

	WHERE
		(InvestorID = @InvestorID OR ISNULL(@InvestorID, 0) = 0)
		AND
		RuleType = @RuleType
END
GO

ALTER PROCEDURE I_GetInvestorLoanCashRequest
	@CashRequestsId bigint
AS
BEGIN
  SELECT
    cr.id as CashRequestsId,
    cr.IdCustomer as CustomerID,
	ipt.FundingTypeID ,
    cr.ManagerApprovedSum    
  FROM CashRequests cr
  INNER JOIN I_ProductSubType ipt
    ON cr.ProductSubTypeID = ipt.ProductSubTypeID
  WHERE cr.Id = @CashRequestsId
END
GO

ALTER PROCEDURE I_GetInvestorsBalance
AS
BEGIN

		;WITH last_balance as 
		(
			SELECT
			    MAX(iisb.Timestamp) AS LastTimeStamp,
				iiba.InvestorID
			FROM I_InvestorSystemBalance iisb
			INNER JOIN I_InvestorBankAccount iiba
			ON iiba.InvestorBankAccountID = iisb.InvestorBankAccountID and iiba.IsActive =1
			WHERE iiba.InvestorAccountTypeID = 1
			GROUP BY InvestorID
		)
  SELECT
    iiba.InvestorID,
    iisb.NewBalance AS Balance
  FROM I_InvestorSystemBalance iisb INNER JOIN I_InvestorBankAccount iiba
      ON iiba.InvestorBankAccountID = iisb.InvestorBankAccountID and iiba.IsActive =1
	INNER JOIN last_balance lb ON lb.InvestorID = iiba.InvestorID
  WHERE lb.LastTimeStamp = iisb.Timestamp 
END
GO

ALTER PROCEDURE I_GetInvestorsIds
AS
BEGIN
  SELECT
    InvestorID as Value
  FROM I_Investor i
  WHERE i.IsActive = 1
END
GO


ALTER PROCEDURE I_GetInvestorParametersDB 
@TypeID int,
@InvestorID int = NULL
AS
BEGIN
  SELECT
	[InvestorParamsID]
	,[InvestorID]
	,[ParameterID]
	,[Value]
	,[Type]
	,[AllowedForConfig]
  FROM I_InvestorParams
  WHERE (InvestorID = @InvestorID OR ISNULL(@InvestorID, 0) = 0)
  AND type = @TypeID
END
GO




ALTER PROCEDURE I_GetGradeMonthlyInvestedAmount 
@InvestorID int,
@GradeID int,
@FirstOfMonth DATETIME
AS
BEGIN
  DECLARE @GradeSum DECIMAL = 0
  
  SELECT
    @GradeSum = isnull(SUM(l.LoanAmount), 0)
  FROM I_Portfolio ip
  INNER JOIN Loan l
    ON ip.LoanID = l.Id
  WHERE ip.InvestorID = @InvestorID
  AND ip.GradeID = @GradeID
  AND ip.Timestamp >= @FirstOfMonth
  GROUP BY ip.GradeID
  
  SELECT @GradeSum AS GradeSum
END
GO


ALTER PROCEDURE I_IndexLoad 
@InvestorID int = NULL
AS
BEGIN
	
  SELECT
    IndexID
    ,InvestorID
    ,ProductTypeID
    ,IsActive
    ,GradeAPercent
    ,GradeAMinScore
    ,GradeAMaxScore
    ,GradeBPercent
    ,GradeBMinScore
    ,GradeBMaxScore
    ,GradeCPercent
    ,GradeCMinScore
    ,GradeCMaxScore
    ,GradeDPercent
    ,GradeDMinScore
    ,GradeDMaxScore
    ,GradeEPercent
    ,GradeEMinScore
    ,GradeEMaxScore
    ,GradeFPercent
    ,GradeFMinScore
    ,GradeFMaxScore
    ,GradeGPercent
    ,GradeGMinScore
    ,GradeGMaxScore
    ,GradeHPercent
    ,GradeHMinScore
    ,GradeHMaxScore
    ,Timestamp
  FROM I_Index
  WHERE (InvestorID = @InvestorID OR ISNULL(@InvestorID, 0) = 0) 
  AND IsActive=1
END
GO



ALTER PROCEDURE I_GetInvestorTotalMonthlyDeposits 
@InvestorID int,
@FirstOfMonth DATETIME
AS
BEGIN
  DECLARE @Balance DECIMAL(18,6) = (
	  SELECT TOP 1
		NewBalance
	  FROM I_InvestorSystemBalance iisb
	  INNER JOIN I_InvestorBankAccount iiba
		ON iiba.InvestorBankAccountID = iisb.InvestorBankAccountID and iiba.IsActive =1
	  WHERE iiba.InvestorID = 1
	  AND iisb.Timestamp < @FirstOfMonth
	  ORDER BY iiba.Timestamp DESC
  )
  
  IF @Balance IS NULL
  	SET @Balance = 0

  SELECT
    (SUM(iibat.TransactionAmount) + @Balance) AS Balance
  FROM I_InvestorBankAccountTransaction iibat
  INNER JOIN I_InvestorBankAccount iiba
    ON iiba.InvestorBankAccountID = iibat.InvestorBankAccountID and iiba.IsActive =1
  WHERE iibat.TransactionAmount > 0
  AND iiba.InvestorID = @InvestorID
  AND iibat.Timestamp >= @FirstOfMonth
END
GO

ALTER PROCEDURE I_GetInvestorMonthlyFundingCapital
 @InvestorID int
AS
BEGIN
	DECLARE @MonthlyFundingCapital DECIMAL(18,6)
	SELECT
		@MonthlyFundingCapital = isnull(MonthlyFundingCapital,0)
	FROM 
		I_Investor
	WHERE 
		InvestorID = @InvestorID
	-------------------------------------------
	SELECT 
		isnull(@MonthlyFundingCapital, 0) MonthlyFundingCapital
END
GO

ALTER PROCEDURE I_GetFundedAmountPeriod 
@InvestorID INT,
@PeriodAgo DATETIME
AS
BEGIN
  SELECT
    isnull(SUM(l.LoanAmount), 0) AS InvestmentAmount
  FROM I_Portfolio ip
  INNER JOIN Loan l
    ON ip.LoanID = l.Id
  WHERE ip.InvestorID = @InvestorID
  AND ip.Timestamp >= @PeriodAgo
END
GO

ALTER PROCEDURE I_GetInvestorWithLatestLoanDate 
@InvestorIDs IntList READONLY
AS
BEGIN
		;WITH latestLoans as 
		(
			SELECT 
				ids.Value AS InvestorID, 
				MAX(ip.Timestamp) AS TakenDate 
			FROM
				@InvestorIDs ids 
			LEFT JOIN 
				I_Portfolio ip ON ip.InvestorID = ids.Value
			GROUP BY 
				ids.Value
		)
	SELECT top 1 ll.InvestorID from latestLoans ll order by ll.TakenDate ASC
END

GO

ALTER PROCEDURE I_GetInvestorParameters 
AS
BEGIN
SELECT ParameterID
	   ,Name
	   ,ValueType
	   ,DefaultValue
	   ,MaxLimit
	   ,MinLimit
	   ,TimestampCounter
  FROM dbo.I_Parameter
END
GO