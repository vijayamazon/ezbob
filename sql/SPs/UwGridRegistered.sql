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
		ISNULL(t.Name, '') AS MpTypeName,
		(CASE
			WHEN m.UpdatingStart IS NULL THEN 'Never started'
			WHEN m.UpdateError IS NOT NULL AND LTRIM(RTRIM(m.UpdateError)) != '' THEN 'Error'
			WHEN m.UpdatingStart IS NOT NULL AND m.UpdatingEnd IS NULL THEN 'In progress'
			ELSE 'Done'
		END) AS MpStatus,	
		w.WizardStepTypeDescription AS WizardStep,
		(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
		(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
		LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
		LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
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
		c.Id DESC,
		t.Id
END
GO
