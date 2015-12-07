IF OBJECT_ID('UwGridPendingInvestor') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridPendingInvestor AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UwGridPendingInvestor
@WithTest BIT


AS
BEGIN
	SET NOCOUNT ON

	SELECT 
		
		c.Id AS CustomerID,
		ISNULL(c.Fullname, '') AS Name,
		'A' AS Grade,
		100 AS ApplicantScore,		
		c.CreditSum AS ApprovedAmount,
		r.RepaymentPeriod AS Term,
		r.UnderwriterDecisionDate AS RequestApprovedAt,
		r.UnderwriterDecisionDate AS TimeLimitUntilAutoreject,
		'Find Investor' AS FindInvestor,
		'Edit Offer' AS EditOffer,
		'ChooseInvestorCombo' AS ChooseInvestor,
	    'Submit' AS SubmitChoosenInvestor,
		'Manage' AS ManageChoosenInvestor

	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id
	WHERE c.CreditResult = 'PendingInvestor' 
	ORDER BY
	c.Id
	DESC
END
GO
