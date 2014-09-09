IF OBJECT_ID('RptBrokerClients') IS NULL
	EXECUTE('CREATE PROCEDURE RptBrokerClients AS SELECT 1')
GO

ALTER PROCEDURE RptBrokerClients
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

SELECT x.Id, x.Status, x.FullName, x.Name, x.DaytimePhone, x.MobilePhone, x.GreetingMailSentDate, x.BrokerID, x.ContactName, x.ContactMobile, x.FirmName, 
   CASE WHEN x.ManagerApprovedSum = 0 THEN NULL ELSE x.ManagerApprovedSum END ApprovedAmount, 
   CASE WHEN x.AmountTaken = 0 THEN NULL ELSE x.AmountTaken END TakenAmount FROM
(
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
			WHEN W.WizardStepTypeName = 'link' THEN 'Link Accounts'
			WHEN W.WizardStepTypeName = 'companydetails' THEN 'Company Details'
			WHEN W.WizardStepTypeName = 'details' THEN 'Personal Details'
			WHEN W.WizardStepTypeName = 'signup' THEN 'Application Step 1'
			ELSE 'Unknown: ' + W.WizardStepTypeName
			END AS Status,
		CASE 
			WHEN W.WizardStepTypeName = 'success' THEN
			 	CASE
			 		WHEN C.LastLoanDate IS NOT NULL THEN 10
			 	ELSE 
			 		CASE 
			 			WHEN C.NumApproves > 0 THEN 9
			 		ELSE 8 END
			 	END	
			WHEN W.WizardStepTypeName = 'link' THEN 7
			WHEN W.WizardStepTypeName = 'companydetails' THEN 6
			WHEN W.WizardStepTypeName = 'details' THEN 5
			WHEN W.WizardStepTypeName = 'signup' THEN 4
			ELSE 3
			END AS StatusNum,	
		C.FullName,
		C.Name,
		C.DaytimePhone,
		C.MobilePhone,
		C.GreetingMailSentDate,
		B.BrokerID,
		B.ContactName,
		B.ContactMobile,
		B.FirmName,
		CAST(C.ManagerApprovedSum AS INT) ManagerApprovedSum,
		CAST(C.AmountTaken AS INT) AmountTaken
		
	FROM
		Customer C
		INNER JOIN Broker B ON B.BrokerID = C.BrokerID
		INNER JOIN WizardStepTypes W ON W.WizardStepTypeID = C.WizardStep
		
	WHERE
		C.IsTest = 0
		AND
		@DateStart <= C.GreetingMailSentDate AND C.GreetingMailSentDate < @DateEnd
) x
ORDER BY x.StatusNum DESC, x.Status		
END
GO
