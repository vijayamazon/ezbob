IF OBJECT_ID('GetLastOfferDataForReApproval') IS NULL
	EXECUTE('CREATE PROCEDURE GetLastOfferDataForReApproval AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetLastOfferDataForReApproval
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE
		@OfferValidUntil DATETIME, 
		@OfferStart DATETIME,
		@PrincipalPaidAmountOld FLOAT,
		@ManualDecisionDate DATETIME,
		@NumOfMPsAddedOld INT,
		@SumOfChargesOld INT

	------------------------------------------------------------------------------

	SELECT 
		@ManualDecisionDate = MAX(cr.UnderwriterDecisionDate)
	FROM 
		CashRequests cr 
	WHERE 
		cr.IdCustomer = @CustomerId
		AND 
		cr.UnderwriterDecision = 'Approved'

	------------------------------------------------------------------------------

	SELECT 
		@NumOfMPsAddedOld = COUNT(cmp.Id)
	FROM
		CashRequests cr
		LEFT JOIN MP_CustomerMarketPlace cmp ON cmp.CustomerId = cr.IdCustomer
	WHERE
		cr.IdCustomer = @CustomerId AND
		@ManualDecisionDate <= (SELECT max(created) FROM MP_CustomerMarketPlace WHERE CustomerId=@CustomerId)

	------------------------------------------------------------------------------

	SELECT 
		@SumOfChargesOld =  sum(lc.Amount)
	FROM
		Loan l
		LEFT JOIN LoanCharges lc ON lc.LoanId = l.id
	WHERE
		l.Customerid = @CustomerId AND
		@ManualDecisionDate < lc.Date

	------------------------------------------------------------------------------

	SELECT
		@OfferValidUntil = ValidFor,
		@OfferStart = ApplyForLoan
	FROM
		Customer
	WHERE
		Id = @CustomerId

	------------------------------------------------------------------------------

	SELECT
		@PrincipalPaidAmountOld = sum(lt.LoanRepayment)
	FROM
		Loan l
		LEFT JOIN LoanTransaction lt ON lt.LoanId = l.Id
	WHERE
		l.Customerid = @CustomerId AND
		@ManualDecisionDate <= lt.PostDate AND
		lt.Status = 'Done' AND
		Type ='PaypointTransaction'

	------------------------------------------------------------------------------

	SELECT
		EmailSendingBanned,
		@OfferStart AS OfferStart,
		@OfferValidUntil AS OfferValidUntil,
		@PrincipalPaidAmountOld AS 'PrincipalPaidAmountOld',
		@NumOfMPsAddedOld AS 'NumOfMPsAddedOld',
		@SumOfChargesOld AS 'SumOfChargesOld',
		SystemCalculatedSum
	FROM
		CashRequests cr
	WHERE
		Id =
			(
			SELECT
				Id
			FROM
				(
				SELECT
					ROW_NUMBER() OVER (ORDER BY Id DESC) AS row,
					cr.Id
				FROM
					CashRequests cr
				WHERE
					cr.IdCustomer =@CustomerId
				) p
			WHERE
				p.row=2
			)
END
GO
