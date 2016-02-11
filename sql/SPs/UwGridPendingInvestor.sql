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

	;WITH 
	last_cr AS (
		SELECT 
			MAX(cr.Id) maxid  
		FROM 
			CashRequests cr 
		INNER JOIN 
			Customer c ON cr.IdCustomer = c.Id
		WHERE 
			c.CreditResult = 'PendingInvestor'	
		GROUP BY
			cr.IdCustomer
	)
	SELECT 
		c.Id AS CustomerID,
		ISNULL(c.Fullname, '') AS Name,
		g.Name AS Grade,
		lg.Score AS ApplicantScore,		
		r.ManagerApprovedSum AS ApprovedAmount,
		r.RepaymentPeriod AS Term,
		r.UnderwriterDecisionDate AS RequestApprovedAt,
		r.OfferValidUntil AS TimeLimitUntilAutoreject,
		'Find Investor' AS FindInvestor,
		'Edit Offer' AS EditOffer,
		'ChooseInvestorCombo' AS ChooseInvestor,
	    'Submit' AS SubmitChosenInvestor,
		'Config' AS ManageChosenInvestor,
		r.Id AS CashRequestID
	FROM
		CashRequests r
		INNER JOIN 
			Customer c ON r.IdCustomer = c.Id
		INNER JOIN 
			last_cr ON r.Id = last_cr.maxid
		LEFT JOIN 
			MP_ServiceLog s ON s.CustomerId = c.Id AND s.ServiceType='LogicalGlue'
		LEFT JOIN 
			CustomerLogicalGlueHistory lg ON lg.CustomerID = c.Id AND lg.IsActive=1
		LEFT JOIN 
			I_Grade g ON g.GradeID = lg.GradeID
	WHERE
		c.CreditResult = 'PendingInvestor' 
	AND 
		r.UnderwriterDecision = 'PendingInvestor'
	AND
		(@WithTest = 1 OR c.IsTest = 0)
	ORDER BY
		c.Id DESC
END


GO
