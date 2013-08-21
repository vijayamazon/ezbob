IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptCallManagement]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptCallManagement]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptCallManagement
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
	 OR C.IsTest=1)

----------- GET Loan Data---------
IF OBJECT_ID('tempdb..#temp2') IS NOT NULL DROP TABLE #temp2

SELECT c.Id cId, l.Status LoanStatus, count(l.Id) AmountOfLoans, ISNULL(sum(l.Balance), 0) OutstandingBalance
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
					OR C.IsTest=1)
GROUP BY c.Id, l.Status
HAVING count(l.Id) > 0


--------- FINAL TABLE MERGE ----------

SELECT T1.eMail, T1.name, T1.Status, T1.OpenOffer, T2.AmountOfLoans, T2.OutstandingBalance,
T2.LoanStatus, T1.CRMStatus, T1.CRMAction, T1.CRMComment
FROM #temp2 T2  
LEFT JOIN #temp1 T1 ON T1.cId = T2.cId


DROP TABLE #temp1
DROP TABLE #temp2

END
GO
