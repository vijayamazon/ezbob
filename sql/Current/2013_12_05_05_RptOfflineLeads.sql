IF OBJECT_ID('RptOfflineLeads') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE RptOfflineLeads AS SELECT 1')
END
GO

ALTER PROCEDURE RptOfflineLeads
@DateStart DATETIME,
@DateEnd DATETIME
AS
SELECT
	c.Id,
	c.GreetingMailSentDate,
	c.CreditResult,
	c.Name,
	c.Fullname,
	w.WizardStepTypeDescription,
	c.DaytimePhone,
	c.MobilePhone,
	c.OverallTurnOver
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
WHERE
	@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
	AND
	c.IsTest = 0
	AND
	c.IsOffline = 1
ORDER BY
	c.GreetingMailSentDate,
	c.Fullname
GO
