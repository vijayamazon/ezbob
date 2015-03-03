IF OBJECT_ID('UwGridRegistered') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridRegistered AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UwGridRegistered
@WithTest BIT,
@WithAll BIT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.Id AS CustomerID,
		c.Name AS Email,
		(CASE w.TheLastOne WHEN 1 THEN 'credit calculation' ELSE 'registered' END) AS UserStatus,
		c.GreetingMailSentDate AS RegDate,
		'' AS MpTypeName,
		dbo.udfGetMpsStatuses(c.Id) AS MpStatus,	
		w.WizardStepTypeDescription AS WizardStep,
		(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
		(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	WHERE
		(
			@WithTest = 1 OR c.IsTest = 0
		)
		AND
		c.CreditResult IS NULL
		AND
		(
			@WithAll = 1 OR c.GreetingMailSentDate >= DATEADD(day, -7, @Now)
		)
	ORDER BY
		c.Id DESC
END
GO
