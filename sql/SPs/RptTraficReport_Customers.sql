IF OBJECT_ID('RptTraficReport_Customers') IS NULL
	EXECUTE('CREATE PROCEDURE RptTraficReport_Customers AS SELECT 1')
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[RptTraficReport_Customers] 
 (@DateStart DATE, @DateEnd DATE)
AS
BEGIN

-- customers

SELECT count(DISTINCT c.Id) Customers, 
       sum(CASE WHEN c.WizardStep=4 THEN 1 ELSE 0 END) AS Applications, 
       sum(CASE WHEN c.NumApproves > 0 THEN 1 ELSE 0 END) AS NumOfApproved,
       sum(CASE WHEN c.NumRejects > 0 AND c.NumApproves=0 THEN 1 ELSE 0 END) AS NumOfRejected, 
	   CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
	   c.GoogleCookie
INTO #temp1	   
FROM Customer c LEFT JOIN Loan l ON l.CustomerId=c.Id 
WHERE c.GreetingMailSentDate>=@DateStart AND c.GreetingMailSentDate<@DateEnd AND c.IsTest=0
GROUP BY c.ReferenceSource, c.GoogleCookie, c.BrokerID

-- loans (new loans)
SELECT
	count(l.Id) NumOfLoans,
	sum(L.loanAmount) LoanAmount, 
	CASE WHEN BrokerID IS NULL THEN C.ReferenceSource ELSE 'brk' END AS ReferenceSource,
	C.GoogleCookie
INTO #temp2	
FROM Loan l INNER JOIN Customer c ON l.CustomerId = c.Id
WHERE 
	c.IsTest = 0 AND 
	l.[Date] > @DateStart AND 
	l.[Date] < @DateEnd AND 
	l.CustomerId NOT IN (SELECT customerId FROM Loan WHERE [Date] < @DateStart)
GROUP BY c.ReferenceSource, c.GoogleCookie, c.BrokerID

SELECT 
	A.Customers, 
	A.Applications, 
	A.NumOfApproved, 
	A.NumOfRejected, 
	B.NumOfLoans, 
	B.LoanAmount, 
	A.ReferenceSource, 
	A.GoogleCookie
FROM #temp1 A FULL OUTER JOIN #temp2 B
ON A.ReferenceSource = B.ReferenceSource AND A.GoogleCookie = B.GoogleCookie

DROP TABLE #temp1
DROP TABLE #temp2
END
GO
