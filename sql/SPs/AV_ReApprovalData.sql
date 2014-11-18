SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_ReApprovalData') IS NULL
	EXECUTE('CREATE PROCEDURE AV_ReApprovalData AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[AV_ReApprovalData] 
	@CustomerId INT
AS
BEGIN

	--is fraud suspect
	DECLARE @IsFraudSuspect BIT = 0
	
	SELECT @IsFraudSuspect = CASE WHEN c.FraudStatus=0 THEN 0 ELSE 1 END 
	FROM Customer c 
	WHERE c.Id=@CustomerId

	--last manual approve
	DECLARE @ManualApproveDate DATETIME 
	DECLARE @OfferedAmount INT	
	DECLARE @ManualApproveRequestId INT = (SELECT max(Id)
  										   FROM CashRequests 
  										   WHERE IdCustomer=@CustomerId 
  										   AND IdUnderwriter IS NOT NULL 
  										   AND UnderwriterDecision='Approved')
  										   
	SELECT @ManualApproveDate = c.UnderwriterDecisionDate, @OfferedAmount = c.ManagerApprovedSum
	FROM CashRequests c
	WHERE Id = @ManualApproveRequestId
	
	--was rejected after last approve
	DECLARE @WasRejected BIT = 0
	IF EXISTS(SELECT * FROM CashRequests 
			  WHERE IdCustomer=@CustomerId 
			  AND UnderwriterDecision='Rejected' 
			  AND Id>@ManualApproveRequestId)
	BEGIN
		SET @WasRejected = 1
	END		  
   	
	--was late
	DECLARE @WasLate BIT = 0
	IF EXISTS (SELECT * 
	            FROM Loan l LEFT JOIN LoanSchedule ls ON l.Id=ls.LoanId 
	            WHERE l.CustomerId=@CustomerId 
	            AND ls.[Date]>@ManualApproveDate 
	            AND (ls.Status='Late' OR ls.Status='Paid')) 
	BEGIN
		SET @WasLate = 1
	END 	
	            
	-- max late days after last approve	            
	DECLARE @MaxLateDays INT = 0
	
	SELECT @MaxLateDays = isnull(max(datediff(day, ls.[Date], lt.PostDate)),0)
	FROM Loan l INNER JOIN LoanScheduleTransaction lst ON lst.LoanID = l.Id
	INNER JOIN LoanTransaction lt ON lt.Id = lst.TransactionID
	INNER JOIN LoanSchedule ls ON ls.Id = lst.ScheduleID
	WHERE l.CustomerId=@CustomerId 
	AND lt.PostDate>@ManualApproveDate

	--new data source was added
	DECLARE @NewDataSourceAdded BIT = 0
	IF EXISTS (SELECT * FROM MP_CustomerMarketPlace WHERE CustomerId=@CustomerId AND Created > @ManualApproveDate) 
	BEGIN
		SET @NewDataSourceAdded = 1
	END	
	
	--num of outstanding loans
	DECLARE @NumOutstandingLoans INT = (SELECT count(*) FROM Loan WHERE CustomerId=@CustomerId AND Status<>'PaidOff')
	
	--loan charges
	DECLARE @HasLoanCharges BIT = 0
	IF EXISTS (SELECT * FROM LoanCharges lc 
			   INNER JOIN Loan l ON l.Id = lc.LoanId 
			   WHERE l.CustomerId=@CustomerId 
			   AND lc.[Date]>@ManualApproveDate)
	BEGIN
		SET @HasLoanCharges = 1
	END		   

	--took a loan from last offer or later
	DECLARE @TookLoanAmount INT = 0
	DECLARE @RepaidPrincipal DECIMAL(18,4) = 0 
	DECLARE @SetupFee DECIMAL(18,4) = 0
	
	SELECT @TookLoanAmount = isnull(sum(LoanAmount),0)
    FROM Loan l 
    WHERE l.CustomerId=@CustomerId 
    AND l.RequestCashId>=@ManualApproveRequestId
	
	SELECT @SetupFee = isnull(sum(lt.Fees),0) FROM Loan l INNER JOIN LoanTransaction lt ON l.Id=lt.LoanId
	WHERE l.CustomerId=@CustomerId 
    AND l.RequestCashId>=@ManualApproveRequestId
    AND lt.Type='PacnetTransaction' 
    AND lt.Status='Done'
	
	SELECT @RepaidPrincipal = isnull(sum(lt.Principal),0) FROM Loan l INNER JOIN LoanTransaction lt ON l.Id=lt.LoanId
	WHERE l.CustomerId=@CustomerId 
    AND l.RequestCashId>=@ManualApproveRequestId
    AND lt.Type='PaypointTransaction' 
    AND lt.Status='Done'
    
    
    --configuration variables
    DECLARE @AutoReApproveMaxLacrAge INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name='AutoReApproveMaxLacrAge')
    DECLARE @AutoReApproveMaxLatePayment INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name='AutoReApproveMaxLatePayment')
    DECLARE @AutoReApproveMaxNumOfOutstandingLoans INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name='AutoReApproveMaxNumOfOutstandingLoans')
	 
----------------------------data---------------------------------------
SELECT 
		@IsFraudSuspect AS IsFraudSuspect,
		@ManualApproveDate AS ManualApproveDate, 
		@OfferedAmount AS OfferedAmount, 
		@WasRejected AS WasRejected,
		@WasLate AS WasLate,
		@MaxLateDays AS MaxLateDays,
		@NewDataSourceAdded AS NewDataSourceAdded,
		@NumOutstandingLoans AS NumOutstandingLoans,
		@HasLoanCharges AS HasLoanCharges,
		@TookLoanAmount AS LoanAmount,
		@RepaidPrincipal AS RepaidPrincipal,
		@SetupFee AS SetupFee,
		
		@AutoReApproveMaxLacrAge AS AutoReApproveMaxLacrAge,
		@AutoReApproveMaxLatePayment AS AutoReApproveMaxLatePayment,
		@AutoReApproveMaxNumOfOutstandingLoans AS AutoReApproveMaxNumOfOutstandingLoans
		
END
GO
