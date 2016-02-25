IF OBJECT_ID('UwGridEscalated') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridEscalated AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UwGridEscalated
@WithTest BIT
AS
BEGIN
	SELECT
		c.Id AS CustomerID,
		ISNULL(c.MedalType, '') AS Medal,
		ISNULL(g.Name, '') AS Grade,
		dbo.udfGetMpsTypes(c.Id) AS MpTypeName,
		c.ApplyForLoan AS ApplyDate,
		c.GreetingMailSentDate AS RegDate,
		c.Status AS CurrentStatus,
		ISNULL(c.Fullname, '') AS FullName,
		c.Name AS Email,
		w.WizardStepTypeDescription AS WizardStep,
		c.SystemCalculatedSum AS CalcAmount,
		(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
		(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
		(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
		c.LastStatus AS CurrentStatus,
		c.DateEscalated AS EscalationDate,
		c.UnderwriterName AS Underwriter,
		c.EscalationReason AS Reason
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
		LEFT JOIN CustomerLogicalGlueHistory lg ON c.Id = lg.CustomerID AND c.CompanyId = lg.CompanyID AND lg.IsActive = 1
		LEFT JOIN I_Grade g ON lg.GradeID = g.GradeID
	WHERE
		(@WithTest = 1 OR c.IsTest = 0)
		AND
		c.CreditResult = 'Escalated'
	ORDER BY
		c.Id DESC
END
GO
