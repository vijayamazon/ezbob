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
		CASE 
			WHEN W.WizardStepTypeName = 'success' THEN
			 	CASE
			 		WHEN C.LastLoanDate IS NOT NULL THEN 'Loan'
			 	ELSE 
			 		CASE 
			 			WHEN C.NumApproves > 0 THEN 'Approved'
			 		ELSE C.CreditResult END
			 	END	
			WHEN W.WizardStepTypeName = 'link' THEN 'Step 4: Link Accounts'
			WHEN W.WizardStepTypeName = 'companydetails' THEN 'Step 3: Company Details'
			WHEN W.WizardStepTypeName = 'details' THEN 'Step 2: Personal Details'
			WHEN W.WizardStepTypeName = 'signup' THEN 'Step 1: Application'
			END AS Status,
		C.FullName,
		C.Name,
		C.DaytimePhone,
		C.MobilePhone,
		C.GreetingMailSentDate,
		B.ContactName,
		B.ContactMobile,
		B.FirmName,
		CASE B.IsTest WHEN 1 THEN 'test' ELSE '' END AS TestBroker
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
