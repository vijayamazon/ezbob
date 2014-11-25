IF OBJECT_ID('GetCustomerDataForReRejection') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerDataForReRejection AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerDataForReRejection
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @WasManuallyRejected BIT = 0
	DECLARE @LastManualRejectDate DATETIME
	DECLARE @NewDataSourceAdded BIT = 0
	DECLARE @HasLoans BIT = 0
	DECLARE @OpenLoansAmount INT = 0
	DECLARE @PrincipalRepaymentAmount DECIMAL(18,4) = 0
	DECLARE @SetupFees DECIMAL(18,4) = 0
	------------------------------------------------------------------------------
	
	-- last manual reject date
	SELECT @LastManualRejectDate = MAX(cr.UnderwriterDecisionDate)
	FROM CashRequests cr
	WHERE cr.IdCustomer = @CustomerId 
	AND	cr.IdUnderwriter IS NOT NULL 
	AND	cr.UnderwriterDecision = 'Rejected'
	
	-- has loans
	IF EXISTS (SELECT * FROM Loan WHERE CustomerId=@CustomerId)
	BEGIN
		SET @HasLoans = 1
	END
	
	-- open loans amount
	SELECT @OpenLoansAmount = isnull(sum(LoanAmount), 0)  
	FROM Loan 
	WHERE CustomerId=@CustomerId 
	AND Status<>'PaidOff'
	
	--principal repayment of open loans
	SELECT @PrincipalRepaymentAmount = isnull(sum(lt.LoanRepayment), 0) 
	FROM Loan l INNER JOIN LoanTransaction lt ON l.Id=lt.LoanId
	WHERE l.CustomerId=@CustomerId 
	AND l.Status<>'PaidOff'
	AND lt.Type='PaypointTransaction'
	AND lt.Status='Done'
	
	SELECT @SetupFees = isnull(sum(lt.Fees), 0) 
	FROM Loan l INNER JOIN LoanTransaction lt ON l.Id=lt.LoanId
	WHERE l.CustomerId=@CustomerId 
	AND l.Status<>'PaidOff'
	AND lt.Type='PacnetTransaction'
	AND lt.Status='Done'

	
	IF @LastManualRejectDate IS NOT NULL
	BEGIN
	
		--manual rejected
		SET @WasManuallyRejected = 1
		
		-- added new mp after last reject
		IF EXISTS (SELECT * FROM MP_CustomerMarketPlace mp
				   WHERE mp.CustomerId=@CustomerId 
				   AND mp.Disabled=0 
				   AND mp.Created > @LastManualRejectDate)
		BEGIN
			SET @NewDataSourceAdded = 1
		END
	END
	
	SELECT 
		@WasManuallyRejected AS WasManuallyRejected
	   ,@LastManualRejectDate AS LastManualRejectDate
	   ,@NewDataSourceAdded AS NewDataSourceAdded
	   ,@OpenLoansAmount AS OpenLoansAmount
	   ,isnull(@PrincipalRepaymentAmount, 0) + isnull(@SetupFees, 0) AS PrincipalRepaymentAmount
	   ,@HasLoans AS HasLoans
END

GO

