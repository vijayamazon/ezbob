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
	SET @DateEnd = CONVERT(DATE, @DateStart)
	SET @DateStart = CONVERT(DATE, @DateEnd)

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
	--WHERE @DateStart <= O.CreationDate AND O.CreationDate < @DateEnd
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
SELECT c.Id cId, count(l.Id) NumberOfLoans, l.DateClosed DateClosed
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
GROUP BY c.Id, l.DateClosed

--------------------------Final Merge--------------------------
	SELECT 
		C.Id,
		C.Name AS Email,
		C.FullName Name,
		L.NumberOfLoans,  
	   	max(O.ManagerApprovedSum) MaxApprovedSum,
		
		C.DaytimePhone DayPhone,
		C.MobilePhone MobilePhone,
      
		CR.CRMStatus CRMStatus,
		CR.CRMAction CRMAction,
		CR.CRMComment CRMComment
	FROM
		#FinishedLoans L 
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
		
	--TODO: Add check that was not late when EZ-886 is finished
	--AND @DateStart <= O.CreationDate 
	--AND O.CreationDate < @DateEnd
	GROUP BY C.Id, C.Name, C.FullName,L.NumberOfLoans,C.DaytimePhone, C.MobilePhone, CR.CRMStatus, CR.CRMAction, CR.CRMComment,L.DateClosed
	ORDER BY L.DateClosed DESC 
	
	DROP TABLE #CRMNotes
	DROP TABLE #CRMFinal
	DROP TABLE #FinishedLoans
END
GO
