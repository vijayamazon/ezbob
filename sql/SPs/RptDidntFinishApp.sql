IF OBJECT_ID('RptDidntFinishApp') IS NULL
	EXECUTE('CREATE PROCEDURE RptDidntFinishApp AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptDidntFinishApp
   			   		(@DateStart DATETIME,
					 @DateEnd   DATETIME)

AS
BEGIN

SET NOCOUNT ON

if OBJECT_ID('tempdb..#temp1') is not NULL
BEGIN
	DROP TABLE #temp1
END

if OBJECT_ID('tempdb..#CRMNotes') is not NULL 
BEGIN
	DROP TABLE #CRMNotes
END

if OBJECT_ID('tempdb..#MaxNoteDate') is not NULL 
BEGIN
	DROP TABLE #MaxNoteDate
END

---------------- DIDN'T FINISH APPLICATION (NO BROKER) FOR 2014 --------------
if OBJECT_ID('tempdb..#temp1') is not NULL
BEGIN
	DROP TABLE #temp1
END

if OBJECT_ID('tempdb..#temp2') is not NULL
BEGIN
	DROP TABLE #temp2
END


SELECT C.Id AS CustomerId,
	   C.Fullname,
	   C.GreetingMailSentDate AS SignUpDate,
	   C.Name AS EmailAddress,
	   W.WizardStepTypeDescription AS WizardStep,
	   C.DaytimePhone,
	   C.MobilePhone,
/*	   CASE
	   WHEN max(E.EventTime) < C.GreetingMailSentDate THEN '2000-01-01'
	   WHEN max(E.EventTime) IS NULL THEN '2000-01-01'
	   ELSE max(E.EventTime)
       END AS LastActionDate,
*/     A.PersonalScore,
       A.NumOfDefaults,
       A.CompanyScore,
       A.ThinFile
       
INTO #temp1
	   
FROM Customer C

JOIN WizardStepTypes W ON W.WizardStepTypeID = C.WizardStep
LEFT JOIN CustomerAnalytics A ON A.CustomerID = C.Id

WHERE C.Id NOT IN
	   				(SELECT C.Id
					 FROM Customer C 
					 WHERE Name LIKE '%ezbob%'
					 OR Name LIKE '%liatvanir%'
					 OR Name LIKE '%q@q%'
					 OR Name LIKE '%1@1%'
					 OR C.IsTest=1)
	  AND C.WizardStep !=4
	  AND C.BrokerID IS NULL
	  AND C.GreetingMailSentDate >= @DateStart
	  AND C.GreetingMailSentDate <= @DateEnd

GROUP BY   C.Id,
		   C.Fullname,
		   C.GreetingMailSentDate,
		   C.Name,
		   W.WizardStepTypeDescription,
		   C.DaytimePhone,
		   C.MobilePhone,
		   A.PersonalScore,
       	   A.NumOfDefaults,
       	   A.CompanyScore,
		   A.ThinFile	


--------------- GET LATEST CRM NOTES --------------

SELECT R.CustomerId,
	   max(R.Timestamp) AS NoteDate
INTO #MaxNoteDate
FROM CustomerRelations R
JOIN #temp1 T ON T.CustomerId = R.CustomerId
GROUP BY R.CustomerId


SELECT N.CustomerId,
	   N.NoteDate,	
	   R.Comment,
	   R.UserName,
	   S.Name AS CRMStatus,
	   A.Name AS CRMAction
INTO #CRMNotes
FROM #MaxNoteDate N
JOIN CustomerRelations R ON R.CustomerId = N.CustomerId
JOIN CRMStatuses S ON S.Id = R.StatusId
JOIN CRMActions A ON A.Id = R.ActionId
WHERE R.Timestamp = N.NoteDate

----------------------- FINAL TABLE -------------------

SELECT T1.CustomerId,
	   T1.Fullname,
	   T1.SignUpDate,
	   T1.EmailAddress,
	   T1.WizardStep,
	   T1.DaytimePhone,
	   T1.MobilePhone,
	   T1.PersonalScore,
       T1.NumOfDefaults,
       T1.CompanyScore,
       T1.ThinFile,
	   N.NoteDate AS CRMNoteDate,
	   N.UserName AS CRMUsername,
	   N.Comment AS CRMComment,
	   N.CRMStatus,
	   N.CRMAction
        
FROM #temp1 T1
LEFT JOIN #CRMNotes N ON N.CustomerId = T1.CustomerId

WHERE (T1.PersonalScore IS NULL OR T1.PersonalScore >= 550)
	  AND (T1.CompanyScore IS NULL OR T1.CompanyScore >= 15)
	  AND (T1.NumOfDefaults IS NULL OR T1.NumOfDefaults <=2)

END
GO

