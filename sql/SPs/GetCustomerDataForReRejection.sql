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

	------------------------------------------------------------------------------
	-- last reject date (automatic or manual)
	SELECT
		@LastRejectDate = MAX(cr.UnderwriterDecisionDate)
	FROM
		CashRequests cr
		LEFT JOIN Decisions d ON d.DecisionID = cr.AutoDecisionID
	WHERE
		cr.IdCustomer = @CustomerId 
		AND 
		cr.UnderwriterDecision = 'Rejected'
		AND (
			cr.AutoDecisionID IS NULL
			OR
			d.DecisionName != 'ReReject'
		)

	-- last decision
	SELECT TOP 1
		@LastDecisionDate = cr.UnderwriterDecisionDate,
		@LastDecisionWasReject = CASE WHEN cr.UnderwriterDecision = 'Rejected' THEN 1 ELSE 0 END
	FROM
		CashRequests cr
	WHERE
		cr.IdCustomer = @CustomerId
		AND
		cr.UnderwriterDecisionDate IS NOT NULL
		AND
		cr.UnderwriterDecision IN ('Approved', 'Rejected')
	ORDER BY
		cr.UnderwriterDecisionDate DESC

	-- open loans amount
	SELECT
		@OpenLoansAmount = ISNULL(SUM(LoanAmount), 0),
		@NumOfOpenLoans = COUNT(*)
	FROM
		Loan
	WHERE
		CustomerId = @CustomerId
		AND
		Status != 'PaidOff'

	-- principal repayment of open loans
	SELECT
		@PrincipalRepaymentAmount = ISNULL(SUM(lt.LoanRepayment), 0)
	FROM
		Loan l
		INNER JOIN LoanTransaction lt ON l.Id = lt.LoanId
	WHERE
		l.CustomerId = @CustomerId
		AND
		l.Status != 'PaidOff'
		AND
		lt.Type = 'PaypointTransaction'
		AND
		lt.Status = 'Done'

	-- added new mp after last decision
	IF @LastDecisionDate IS NOT NULL AND EXISTS (
		SELECT * FROM MP_CustomerMarketPlace mp
		WHERE mp.CustomerId = @CustomerId
		AND mp.Disabled = 0
		AND mp.Created > @LastDecisionDate
	)
	BEGIN
		SET @NewDataSourceAdded = 1
	END

	SELECT 
		@LastRejectDate AS LastRejectDate,
		@LastDecisionDate AS LastDecisionDate,
		ISNULL(@LastDecisionWasReject, 0) AS LastDecisionWasReject,
		@NewDataSourceAdded AS NewDataSourceAdded,
		@NumOfOpenLoans AS NumOfOpenLoans,
		@OpenLoansAmount AS OpenLoansAmount,
		ISNULL(@PrincipalRepaymentAmount, 0) AS PrincipalRepaymentAmount
END
GO
