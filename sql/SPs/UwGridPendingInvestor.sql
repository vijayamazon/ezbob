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
	),
	last_lg AS (
		SELECT max(lr.ResponseID) maxid
		FROM 
			LogicalGlueResponses lr 
		INNER JOIN 
			MP_ServiceLog s ON s.Id = lr.ServiceLogID
		INNER JOIN 
			LogicalGlueRequests lq ON lq.ServiceLogID = s.Id
		INNER JOIN 
			Customer c ON s.CustomerId = c.Id
		WHERE 
			c.CreditResult = 'PendingInvestor' AND lq.IsTryOut=0	
		GROUP BY c.Id	
	)
	SELECT 
		c.Id AS CustomerID,
		ISNULL(c.Fullname, '') AS Name,
		g.Name AS Grade,
		lo.Score AS ApplicantScore,		
		r.ManagerApprovedSum AS ApprovedAmount,
		r.RepaymentPeriod AS Term,
		r.UnderwriterDecisionDate AS RequestApprovedAt,
		r.UnderwriterDecisionDate AS TimeLimitUntilAutoreject,
		'Find Investor' AS FindInvestor,
		'Edit Offer' AS EditOffer,
		'ChooseInvestorCombo' AS ChooseInvestor,
	    'Submit' AS SubmitChosenInvestor,
		'Manage' AS ManageChosenInvestor
	FROM
		CashRequests r
		INNER JOIN 
			Customer c ON r.IdCustomer = c.Id
		INNER JOIN 
			last_cr ON r.Id = last_cr.maxid
		LEFT JOIN 
			MP_ServiceLog s ON s.CustomerId = c.Id AND s.ServiceType='LogicalGlue'
		LEFT JOIN 
			LogicalGlueResponses lr ON lr.ServiceLogID = s.Id
		INNER JOIN 
			last_lg ON lr.ResponseID = last_lg.maxid
		LEFT JOIN 
			LogicalGlueModelOutputs lo ON lo.ResponseID = lr.ResponseID AND lo.ModelID=2 --Neural network
		LEFT JOIN 
			I_Grade g ON g.GradeID = lr.GradeID
	WHERE
		c.CreditResult = 'PendingInvestor' 
	AND 
		r.UnderwriterDecision = 'PendingInvestor'
	ORDER BY
		c.Id DESC
END

GO
