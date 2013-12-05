IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerDataForReRejection]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerDataForReRejection]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomerDataForReRejection]
	(@CustomerId INT)
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
			@ManualDecisionDate >= (SELECT cast(max(created) AS DATE) FROM MP_CustomerMarketPlace WHERE CustomerId=@CustomerId) AND 
			DATEADD(DD, 30, @ManualDecisionDate) >= GETUTCDATE() AND l.Id IS NULL AND 
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
		@ManualDecisionDate >= (SELECT cast(max(created)AS DATE) FROM MP_CustomerMarketPlace WHERE CustomerId=@CustomerId) AND 
		DATEADD(DD, 30, @ManualDecisionDate) >= GETUTCDATE() AND 
		l.Id IS NOT NULL AND 
		@ManualDecisionDate = (SELECT cast(max(cr.UnderwriterDecisionDate) AS DATE) FROM CashRequests cr WHERE cr.IdCustomer = @CustomerId) 
	) AS OldCustomer_ReReject,

	(
	SELECT 
		sum(lt.LoanRepayment) AS PrincipalPaidAmount
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
		sum(l.LoanAmount)
	FROM 
		Loan l
	WHERE 
		l.Customerid = @CustomerId
	) AS LoanAmountTaken
END
GO
