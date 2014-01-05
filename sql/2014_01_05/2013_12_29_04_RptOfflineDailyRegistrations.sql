IF OBJECT_ID('RptOfflineDailyRegistrations') IS NULL
	EXECUTE('CREATE PROCEDURE RptOfflineDailyRegistrations')
GO

ALTER PROCEDURE RptOfflineDailyRegistrations
@DateStart DATE,
@DateEnd DATE
AS
	SELECT
		c.Id,
		c.Name,
		c.GreetingMailSentDate,
		w.WizardStepTypeDescription AS WizardStep
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	WHERE
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd 
		AND
		c.IsOffline = 1
		AND
		c.IsTest = 0 
	ORDER BY
		c.WizardStep DESC
GO

