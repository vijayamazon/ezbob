IF OBJECT_ID('GetCustomerDataForReRejection') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerDataForReRejection AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerDataForReRejection
@CustomerId INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ManualDecisionDate DATE

	------------------------------------------------------------------------------

	SELECT
		@ManualDecisionDate = MAX(cr.UnderwriterDecisionDate)
	FROM
		CashRequests cr
	WHERE
		cr.IdCustomer = @CustomerId AND
		cr.IdUnderwriter IS NOT NULL AND
		cr.UnderwriterDecision = 'Rejected'

	------------------------------------------------------------------------------

	SELECT
		ISNULL((
			SELECT
				COUNT(cmp.Id)
			FROM
				CashRequests cr
				LEFT JOIN MP_CustomerMarketPlace cmp ON cmp.CustomerId = cr.IdCustomer
				LEFT JOIN Loan l ON l.CustomerId = cmp.CustomerId
			WHERE
				cr.IdCustomer = @CustomerId
				AND
				cr.IdUnderwriter IS NOT NULL
				AND
				cr.UnderwriterDecision = 'Rejected'
				AND
				@ManualDecisionDate >= (SELECT CAST(MAX(created) AS DATE) FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId)
				AND
				DATEADD(DD, 30, @ManualDecisionDate) >= @Now AND l.Id IS NULL
				AND
				@ManualDecisionDate = (SELECT CAST(MAX(cr.UnderwriterDecisionDate) AS DATE) FROM CashRequests cr WHERE cr.IdCustomer = @CustomerId)
		), 0) AS NewCustomer_ReReject,

		ISNULL((
			SELECT
				COUNT(cmp.Id)
			FROM
				CashRequests cr
				LEFT JOIN MP_CustomerMarketPlace cmp ON cmp.CustomerId = cr.IdCustomer
				LEFT JOIN Loan l ON l.CustomerId = cmp.CustomerId
			WHERE
				cr.IdCustomer = @CustomerId
				AND
				cr.IdUnderwriter IS NOT NULL
				AND
				cr.UnderwriterDecision = 'Rejected'
				AND
				@ManualDecisionDate >= (SELECT CAST(MAX(created) AS DATE) FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId)
				AND
				DATEADD(DD, 30, @ManualDecisionDate) >= @Now
				AND
				l.Id IS NOT NULL
				AND
				@ManualDecisionDate = (SELECT CAST(MAX(cr.UnderwriterDecisionDate) AS DATE) FROM CashRequests cr WHERE cr.IdCustomer = @CustomerId)
		), 0) AS OldCustomer_ReReject,

		ISNULL((
			SELECT
				SUM(ISNULL(lt.LoanRepayment, 0)) AS PrincipalPaidAmount
			FROM
				Loan l
				LEFT JOIN LoanTransaction lt ON lt.LoanId = l.Id
			WHERE
				l.Customerid = @CustomerId
				AND
				lt.Status = 'Done'
				AND
				Type ='PaypointTransaction'
		), 0) AS PrincipalPaidAmount,

		ISNULL((
			SELECT
				SUM(ISNULL(l.LoanAmount, 0))
			FROM
				Loan l
			WHERE
				l.Customerid = @CustomerId
		), 0) AS LoanAmountTaken
END
GO
