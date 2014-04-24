IF OBJECT_ID('RptStepOneCustomers') IS NULL
	EXECUTE('CREATE PROCEDURE RptStepOneCustomers AS SELECT 1')
GO

ALTER PROCEDURE RptStepOneCustomers
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN

SELECT x.cId, x.Name eMail,x.FirstName, y.Phone
FROM 
(
SELECT C.Id cId,C.Name,max(substring(e.EventArguments,1,20)) FirstName 
FROM Customer C, UiEvents E 
WHERE E.UiControlID IN(13)  
AND  C.IsTest = 0 
AND E.UserID = C.Id 
AND  C.WizardStep = 1 
AND C.GreetingMailSentDate >= @DateStart 
AND E.EventArguments IS NOT NULL 
AND E.UiActionID = 2 GROUP BY C.Id,C.Name
) x, 
(
SELECT C.Id cId,C.Name,max(substring(e.EventArguments,1,20)) Phone 
FROM Customer C, UiEvents E 
WHERE E.UiControlID IN(22)  
AND  C.IsTest = 0 
AND E.UserID = C.Id 
AND  C.WizardStep = 1 
AND C.GreetingMailSentDate >= @DateStart 
AND E.EventArguments IS NOT NULL 
AND E.UiActionID = 2 
GROUP BY C.Id,C.Name
) y
WHERE x.cId = y.cId
END 

GO
