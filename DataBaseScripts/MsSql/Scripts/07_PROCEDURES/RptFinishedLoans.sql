IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptFinishedLoans]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptFinishedLoans]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptFinishedLoans
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN

SET @DateEnd = CONVERT(DATE, @DateEnd)
SET @DateStart = CONVERT(DATE, @DateStart)

--IF datediff(day, @DateStart, @DateEnd) = 1 SET @DateStart = dateadd(week, -1,@DateEnd)

---------------------------CRM Notes------------------------------ 
 IF OBJECT_ID('tempdb..#CRMNotes') IS NOT NULL DROP TABLE #CRMNotes
 SELECT
  max(CR.Id) CrmId,
  CR.CustomerId
INTO
#CRMNotes
 FROM
  CustomerRelations CR
  INNER JOIN CashRequests O
   ON O.IdCustomer = CR.CustomerId
   AND O.UnderwriterDecision = 'Approved'
  INNER JOIN CRMStatuses sts ON CR.StatusId = sts.Id
 GROUP BY
  CR.CustomerId
  
-------------------------------CRM Final--------------------------
 IF OBJECT_ID('tempdb..#CRMFinal') IS NOT NULL DROP TABLE #CRMFinal
 SELECT
  CR.CustomerId,
  CR.UserName,
  sts.Name CRMStatus,
  CR.Comment CRMComment,
  act.Name CRMAction
 INTO
  #CRMFinal
 FROM
  CustomerRelations CR
  INNER JOIN #CRMNotes N ON CR.Id = N.CrmId
  INNER JOIN CRMStatuses sts ON CR.StatusId = sts.Id
  INNER JOIN CRMActions act ON CR.ActionId = act.Id
  
--------------------------Finished Loans-----------------------------  
IF OBJECT_ID('tempdb..#FinishedLoans') IS NOT NULL DROP TABLE #FinishedLoans
SELECT c.Id cId, count(l.Id) NumberOfLoans, max(l.DateClosed) DateClosed, 'Fully Repaid' AS LoanStatus
INTO #FinishedLoans
FROM Customer c
INNER JOIN Loan l
ON c.Id = l.CustomerId
WHERE l.DateClosed IS NOT NULL
AND c.Id NOT IN 
 (SELECT cc.Id 
  FROM Customer cc 
  INNER JOIN Loan ll 
  ON cc.Id = ll.CustomerId 
  WHERE ll.DateClosed IS NULL 
  GROUP BY cc.Id)
GROUP BY c.Id

----------------------Almost Finished Loans--------------------------
IF OBJECT_ID('tempdb..#CountStillToPay') IS NOT NULL DROP TABLE #CountStillToPay
SELECT LoanId, Count  
INTO #CountStillToPay
FROM (
SELECT LoanId, count(Status) Count  FROM LoanSchedule
WHERE Status = 'StillToPay'
GROUP BY LoanId ) x
WHERE x.Count <=2

IF OBJECT_ID('tempdb..#AlmostFinishedLoans') IS NOT NULL DROP TABLE #AlmostFinishedLoans
SELECT c.Id cId, count(l.Id) NumberOfLoans, NULL DateClosed, (convert(NVARCHAR,max(p.Count)) + ' Installments Left') AS LoanStatus
INTO #AlmostFinishedLoans
FROM Customer c
INNER JOIN Loan l ON c.Id = l.CustomerId
JOIN #CountStillToPay p ON p.LoanId = l.Id
GROUP BY c.Id


IF OBJECT_ID('tempdb..#Merged') IS NOT NULL DROP TABLE #Merged
SELECT x.cId cId, x.NumberOfLoans NumberOfLoans, x.DateClosed DateClosed, x.LoanStatus LoanStatus
INTO #Merged
FROM
(SELECT * FROM #FinishedLoans
UNION 
SELECT * FROM #AlmostFinishedLoans) x



--------------------------MaxApprovedSum-----------------------------  
IF OBJECT_ID('tempdb..#MaxApproved') IS NOT NULL DROP TABLE #MaxApproved
 SELECT 
 ROW_NUMBER() OVER (ORDER BY C.id) AS 'RowNumber',
  C.Id,
  C.Name AS Email,
  C.FullName Name,
  L.NumberOfLoans NumberOfLoans,  
  max(O.ManagerApprovedSum) MaxApprovedSum,
  C.DaytimePhone DayPhone,
  C.MobilePhone MobilePhone,
  CR.CRMStatus CRMStatus,
  CR.CRMAction CRMAction,
  CR.CRMComment CRMComment,
  L.DateClosed DateClosed,
  L.LoanStatus LoanStatus
  INTO #MaxApproved
 FROM
  #Merged L 
  LEFT JOIN Customer C ON C.Id = L.cId
  INNER JOIN CashRequests O
   ON C.Id = O.IdCustomer
   AND O.UnderwriterDecision = 'Approved'
  LEFT JOIN #CRMFinal CR ON CR.CustomerId = O.IdCustomer
 
 WHERE C.CreditResult <> 'Late' -- is late
 AND C.IsWasLate <> 1
 AND   C.Id NOT IN 
   (SELECT C.Id
    FROM Customer C 
    WHERE Name LIKE '%ezbob%'
    OR Name LIKE '%liatvanir%'
    OR Name LIKE '%q@q%'
    OR Name LIKE '%1@1%'
    OR C.IsTest=1)
 AND C.CollectionStatus = 0 -- Is Enabled Good (Disabled/Legal/Fraud/FraudSuspect/Risky/Bad)
 GROUP BY C.Id, C.Name, C.FullName,L.NumberOfLoans,C.DaytimePhone, C.MobilePhone, CR.CRMStatus, CR.CRMAction, CR.CRMComment,L.DateClosed, L.LoanStatus
-------------------------Underwriter Descision-----------------
IF OBJECT_ID('tempdb..#Decision') IS NOT NULL DROP TABLE #Decision;

WITH e AS
(
     SELECT UnderwriterDecision, UnderwriterDecisionDate, IdCustomer cId ,
         ROW_NUMBER() OVER
         (
             PARTITION BY IdCustomer
             ORDER BY UnderwriterDecisionDate DESC
         ) AS Recency
     FROM CashRequests
     WHERE UnderwriterDecisionDate IS NOT NULL
)

SELECT *
INTO #Decision
FROM e
WHERE Recency = 1

--------------------------Final Merge-------------------------- 
 SELECT m.Id Id, m.Email Email, m.Name Name, m.NumberOfLoans NumberOfLoans,m.LoanStatus, m.MaxApprovedSum, m.DayPhone, m.MobilePhone, m.CRMStatus, m.CRMAction, m.CRMComment, d.UnderwriterDecision
 FROM #MaxApproved m
 LEFT JOIN #Decision d ON m.Id = d.cId
 --WHERE DateClosed >= @DateStart AND DateClosed <= @DateEnd
 --GROUP BY m.Id, m.Email, m.Name, m.NumberOfLoans, m.MaxApprovedSum, m.DayPhone, m.MobilePhone, m.CRMStatus, m.CRMAction, m.CRMComment, m.DateClosed, m.LoanStatus, d.UnderwriterDecision
 ORDER BY m.DateClosed DESC, m.LoanStatus
 
 
 ----------------Drop Temp Tables------------------------------
 DROP TABLE #CRMNotes
 DROP TABLE #CRMFinal
 DROP TABLE #FinishedLoans
 DROP TABLE #MaxApproved
 DROP TABLE #AlmostFinishedLoans
 DROP TABLE #CountStillToPay
 DROP TABLE #Merged
 
END
GO
