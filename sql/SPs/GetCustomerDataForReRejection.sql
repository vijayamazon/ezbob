IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerDataForReRejection]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerDataForReRejection]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomerDataForReRejection] 
	(@CustomerId INT, @Now DATETIME)
AS
BEGIN
	DECLARE @ManualDecisionDate DATE

	SELECT 
		@ManualDecisionDate = max(cr.UnderwriterDecisionDate) 
	FROM 
		CashRequests cr 
	WHERE 
		cr.IdCustomer = @CustomerId AND
		cr.IdUnderwriter IS NOT NULL AND 
		cr.UnderwriterDecision = 'Rejected'

	SELECT 
		(
		SELECT 
			COUNT(cmp.Id)
		FROM 
			CashRequests cr 
			LEFT JOIN MP_CustomerMarketPlace cmp ON cmp.CustomerId = cr.IdCustomer
			LEFT JOIN Loan l ON l.CustomerId = cmp.CustomerId 
		WHERE 
			cr.IdCustomer = @CustomerId AND 
			cr.IdUnderwriter IS NOT NULL AND 
			cr.UnderwriterDecision = 'Rejected' AND 
			@ManualDecisionDate >= (SELECT cast(max(created) AS DATE) FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId) AND 
			DATEADD(DD, 30, @ManualDecisionDate) >= @Now AND l.Id IS NULL AND 
			@ManualDecisionDate = (SELECT cast(max(cr.UnderwriterDecisionDate) AS DATE) FROM CashRequests cr WHERE cr.IdCustomer = @CustomerId)
		) AS NewCustomer_ReReject,
 
	(
	SELECT 
		COUNT(cmp.Id)
	FROM 
		CashRequests cr 
		LEFT JOIN MP_CustomerMarketPlace cmp ON cmp.CustomerId = cr.IdCustomer
		LEFT JOIN Loan l ON l.CustomerId = cmp.CustomerId 
	WHERE 
		cr.IdCustomer = @CustomerId AND 
		cr.IdUnderwriter IS NOT NULL AND 
		cr.UnderwriterDecision = 'Rejected' AND 
		@ManualDecisionDate >= (SELECT cast(max(created) AS DATE) FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId) AND 
		DATEADD(DD, 30, @ManualDecisionDate) >= @Now AND 
		l.Id IS NOT NULL AND 
		@ManualDecisionDate = (SELECT cast(max(cr.UnderwriterDecisionDate) AS DATE) FROM CashRequests cr WHERE cr.IdCustomer = @CustomerId) 
	) AS OldCustomer_ReReject,

	(
	SELECT 
		ISNULL(sum(isnull(lt.LoanRepayment, 0)), 0) AS PrincipalPaidAmount
	FROM 
		Loan l
		LEFT JOIN LoanTransaction lt ON lt.LoanId = l.Id
	WHERE 
		l.Customerid = @CustomerId AND 
		lt.Status = 'Done' AND
		Type ='PaypointTransaction'
	) AS PrincipalPaidAmount,

	(
	SELECT 
		ISNULL(sum(isnull(l.LoanAmount, 0)), 0)
	FROM 
		Loan l
	WHERE 
		l.Customerid = @CustomerId
	) AS LoanAmountTaken
END
GO
