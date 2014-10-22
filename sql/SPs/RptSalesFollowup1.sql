IF OBJECT_ID('RptSalesFollowup1') IS NULL
	EXECUTE ('CREATE PROCEDURE RptSalesFollowup1 AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptSalesFollowup1
@DateStart DATE,
@DateEnd DATE
AS
	SELECT c.Id,c.FullName, s.Description AS ResidentialStatus,c.TypeOfBusiness,c.DaytimePhone,c.MobilePhone,c.OverallTurnOver,c.Name,c.GreetingMailSentDate, c.ExperianConsumerScore AS ConsumerScore, a.Score AS CompanyScore
	FROM Customer c
	LEFT JOIN CustomerPropertyStatuses s ON c.PropertyStatusId = s.Id
	LEFT JOIN CustomerAnalyticsCompany a ON a.CustomerID = c.Id
	WHERE WizardStep = 6 
	AND c.Id NOT IN (SELECT CustomerId FROM CustomerRelations)
	AND c.GreetingMailSentDate >= @DateStart 
	AND c.GreetingMailSentDate < @DateEnd 
	AND c.IsTest = 0 ORDER BY c.OverallTurnOver DESC
GO
