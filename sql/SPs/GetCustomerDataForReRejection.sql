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
	
	DECLARE @LastRejectDate DATETIME
	DECLARE @LastDecisionDate DATETIME
	DECLARE @LastDecisionWasReject BIT = 0
	DECLARE @NewDataSourceAdded BIT = 0
	DECLARE @NumOfOpenLoans INT = 0
	DECLARE @OpenLoansAmount INT = 0
	DECLARE @PrincipalRepaymentAmount DECIMAL(18,4) = 0
	DECLARE @SetupFees DECIMAL(18,4) = 0

	------------------------------------------------------------------------------
	-- last reject date (automatic or manual)
	SELECT
		@LastRejectDate = MAX(cr.UnderwriterDecisionDate)
	FROM
		CashRequests cr LEFT JOIN Decisions d ON d.DecisionID = cr.AutoDecisionID
	WHERE
		cr.IdCustomer = @CustomerId 
		AND 
		((cr.IdUnderwriter IS NOT NULL AND cr.UnderwriterDecision='Rejected') OR (d.DecisionName = 'Reject'))
		
	-- last decision
	SELECT
		@LastDecisionDate = MAX(cr.UnderwriterDecisionDate)
	FROM
		CashRequests cr
	WHERE
		cr.IdCustomer = @CustomerId 
		AND
		cr.UnderwriterDecisionDate IS NOT NULL 
		AND
		cr.UnderwriterDecision != 'WaitingForDecision'
		
	-- last decision was reject	
	IF @LastDecisionDate IS NOT NULL
	BEGIN 
		SELECT @LastDecisionWasReject = CASE WHEN cr.UnderwriterDecision='Rejected' THEN 1 ELSE 0 END  
		FROM CashRequests cr 
		WHERE UnderwriterDecisionDate=@LastDecisionDate
	END 
			
	-- open loans amount
	SELECT @OpenLoansAmount = isnull(sum(LoanAmount), 0), @NumOfOpenLoans = count(*)
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

	IF @LastDecisionDate IS NOT NULL
	BEGIN
		-- added new mp after last decision
		IF EXISTS (SELECT * FROM MP_CustomerMarketPlace mp
				   WHERE mp.CustomerId=@CustomerId 
				   AND mp.Disabled=0 
				   AND mp.Created > @LastDecisionDate)
		BEGIN
			SET @NewDataSourceAdded = 1
		END
	END
	
	SELECT 
	    @LastRejectDate AS LastRejectDate
	   ,@LastDecisionDate AS LastDecisionDate
	   ,@LastDecisionWasReject AS LastDecisionWasReject
	   ,@NewDataSourceAdded AS NewDataSourceAdded
	   ,@NumOfOpenLoans AS NumOfOpenLoans
	   ,@OpenLoansAmount AS OpenLoansAmount
	   ,isnull(@PrincipalRepaymentAmount, 0) + isnull(@SetupFees, 0) AS PrincipalRepaymentAmount
END

GO

