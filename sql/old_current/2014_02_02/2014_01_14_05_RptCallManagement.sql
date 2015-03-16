ALTER PROCEDURE RptCallManagement
		@DateStart DATETIME,
		@DateEnd   DATETIME
AS
BEGIN
	SET @DateStart = CONVERT(DATE, @DateStart)
	SET @DateEnd = CONVERT(DATE, @DateEnd)

----------- GET Customer's Data and CRM Data---------
IF OBJECT_ID('tempdb..#temp1') IS NOT NULL DROP TABLE #temp1

SELECT c.Id cId, c.Name eMail, c.Fullname name, c.LastStatus Status, c.ManagerApprovedSum OpenOffer,
(x.Comment) CRMComment, (x.Status) CRMStatus, (x.Action) CRMAction
INTO #temp1
FROM Customer c
LEFT OUTER JOIN 
	(SELECT cr.Id crId, cr.CustomerId CustomerId, cr.Comment Comment, crma.Name Action, crms.Name Status 
	 FROM CustomerRelations cr
	 LEFT JOIN CustomerRelations cr2 ON (cr.CustomerId = cr2.CustomerId AND cr.Id < cr2.Id)
	 LEFT OUTER JOIN CRMActions crma ON crma.Id = cr.ActionId
     LEFT OUTER JOIN  CRMStatuses crms ON crms.Id = cr.StatusId
     WHERE cr2.Id IS NULL
     ) x 
ON c.Id = x.CustomerId
WHERE c.Id NOT IN 
	(SELECT C.Id
	 FROM Customer C 
	 WHERE Name LIKE '%ezbob%'
	 OR Name LIKE '%liatvanir%'
	 OR Name LIKE '%q@q%'
	 OR Name LIKE '%1@1%'
	 OR C.IsTest=1
	 )
AND c.WizardStep = 4  -- finished wizard only

----------- GET Loan Count Data---------
IF OBJECT_ID('tempdb..#temp2') IS NOT NULL DROP TABLE #temp2

SELECT c.Id cId, count(l.Id) AmountOfLoans, ISNULL(sum(l.Balance), 0) OutstandingBalance
INTO #temp2
FROM Customer c
LEFT JOIN Loan l ON c.Id = l.CustomerId
WHERE c.Id NOT IN 
				  ( SELECT C.Id
					FROM Customer C 
					WHERE Name LIKE '%ezbob%'
					OR Name LIKE '%liatvanir%'
					OR Name LIKE '%q@q%'
					OR Name LIKE '%1@1%'
					OR C.IsTest=1
					)
GROUP BY c.Id

----------- GET Loan Status---------
IF OBJECT_ID('tempdb..#temp3') IS NOT NULL DROP TABLE #temp3
SELECT x.cId, 
COALESCE(
  CASE WHEN x.StatusId='0' THEN 'Late' ELSE NULL END, 
  CASE WHEN x.StatusId='1' THEN 'Live' ELSE NULL END,
  CASE WHEN x.StatusId='2' THEN 'PaidOff' ELSE NULL END,
  CASE WHEN x.StatusId='3' THEN '-' ELSE NULL END)
  AS LoanStatus
 INTO #temp3
 FROM 
 (
SELECT t.cId, min(t.Status) StatusId FROM 
(
SELECT c.Id cId, 
  COALESCE(
  CASE WHEN l.Status='Late' THEN '0' ELSE NULL END, 
  CASE WHEN l.Status='Live' THEN '1' ELSE NULL END,
  CASE WHEN l.Status='PaidOff' THEN '2' ELSE NULL END,
  CASE WHEN l.Status IS NULL THEN '3' ELSE NULL END)
  AS Status
FROM Customer c
LEFT JOIN Loan l ON c.Id = l.CustomerId
WHERE c.Id NOT IN 
				  ( SELECT C.Id
					FROM Customer C 
					WHERE Name LIKE '%ezbob%'
					OR Name LIKE '%liatvanir%'
					OR Name LIKE '%q@q%'
					OR Name LIKE '%1@1%'
				  	OR C.IsTest=1
				  )
) t
GROUP BY t.cId
) x

--------- FINAL TABLE MERGE ----------

SELECT T1.cId, T1.eMail, T1.name, T1.Status, T1.OpenOffer, T2.AmountOfLoans, T2.OutstandingBalance,
T3.LoanStatus, T1.CRMStatus, T1.CRMAction, T1.CRMComment
FROM #temp1 T1  
LEFT JOIN #temp2 T2 ON T1.cId = T2.cId
LEFT JOIN #temp3 T3 ON T1.cId = T3.cId


DROP TABLE #temp1
DROP TABLE #temp2
DROP TABLE #temp3

END

GO

