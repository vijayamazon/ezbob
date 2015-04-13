IF OBJECT_ID('RptLeadsAll') IS NULL
	EXECUTE('CREATE PROCEDURE RptLeadsAll AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLeadsAll
@DateStart DATETIME,
@DateEnd DATETIME,
@WithBrokerCustomers BIT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.Id,
		o.Name AS CustomerOrigin,
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
			WHEN c.IsOffline = 0 THEN 'Online'
			WHEN c.IsOffline = 1 THEN 'Offline' 
		END AS Segment,
		CASE
			WHEN c.BrokerID IS NULL THEN c.ReferenceSource
			ELSE 'BROKER'
		END AS ReferenceSource
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
		LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID
	WHERE
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		AND
		c.IsTest = 0
		AND (
			@WithBrokerCustomers = 1
			OR
			c.BrokerID IS NULL
		)
	ORDER BY
		w.TheLastOne DESC,
		w.WizardStepTypeDescription DESC,
		c.GreetingMailSentDate,
		c.Fullname
END

GO