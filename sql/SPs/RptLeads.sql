IF OBJECT_ID('RptLeads') IS NULL
	EXECUTE('CREATE PROCEDURE RptLeads AS SELECT 1')
GO

ALTER PROCEDURE RptLeads
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT
		c.Id,
		c.GreetingMailSentDate,
		c.CreditResult,
		c.Name,
		c.Fullname,
		w.WizardStepTypeDescription,
		c.DaytimePhone,
		c.MobilePhone,
		c.OverallTurnOver,
		CASE 
			WHEN c.IsOffline IS NULL THEN 'None'
			WHEN  c.IsOffline = 0 THEN 'Online'
			WHEN c.IsOffline = 1 THEN 'Offline' 
		END AS Segment,
		CASE
			WHEN c.BrokerID IS NULL THEN c.ReferenceSource
			ELSE 'BROKER'
		END AS ReferenceSource
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	WHERE
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		AND
		c.IsTest = 0
	ORDER BY
		w.TheLastOne DESC,
		w.WizardStepTypeDescription DESC,
		c.GreetingMailSentDate,
		c.Fullname
END
GO