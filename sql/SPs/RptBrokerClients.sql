IF OBJECT_ID('RptBrokerClients') IS NULL
	EXECUTE('CREATE PROCEDURE RptBrokerClients AS SELECT 1')
GO

ALTER PROCEDURE RptBrokerClients
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		C.Id,
		W.WizardStepTypeName,
		C.CreditResult,
		C.FullName,
		C.Name,
		C.DaytimePhone,
		C.MobilePhone,
		C.GreetingMailSentDate,
		B.ContactName,
		B.ContactMobile,
		B.FirmName
	FROM
		Customer C
		INNER JOIN Broker B ON B.BrokerID = C.BrokerID
		INNER JOIN WizardStepTypes W ON W.WizardStepTypeID = C.WizardStep
	WHERE
		C.istest = 0
		AND
		@DateStart <= C.GreetingMailSentDate AND C.GreetingMailSentDate < @DateEnd
END
GO
