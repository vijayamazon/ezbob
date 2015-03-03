IF OBJECT_ID('UwGridAll') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridAll AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UwGridAll
@WithTest BIT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.Id AS CustomerID,
		ISNULL(c.MedalType, '') AS Medal,
		dbo.udfGetMpsTypes(c.Id) AS MpTypeName,
		c.ApplyForLoan AS ApplyDate,
		c.GreetingMailSentDate AS RegDate,
		c.Status AS CustomerStatus,
		ISNULL(c.Fullname, '') AS FullName,
		c.Name AS Email,
		w.WizardStepTypeDescription AS WizardStep,
		c.SystemCalculatedSum AS CalcAmount,
		(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
		(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
		c.ManagerApprovedSum AS ApprovedSum,
		(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	WHERE
		(
			@WithTest = 1 OR c.IsTest = 0
		)
		AND
		c.CreditResult IS NOT NULL
	ORDER BY
		c.Id DESC
		
END
GO
