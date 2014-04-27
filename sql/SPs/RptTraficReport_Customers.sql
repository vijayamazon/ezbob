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

SELECT count(DISTINCT c.Id) Customers, count(l.Id) NumOfLoans, sum(l.LoanAmount) LoanAmount, c.ReferenceSource, c.GoogleCookie 
FROM Customer c LEFT JOIN Loan l ON l.CustomerId=c.Id 
WHERE c.GreetingMailSentDate>=@DateStart AND c.GreetingMailSentDate<@DateEnd AND c.IsTest=0
GROUP BY c.ReferenceSource, c.GoogleCookie

END
GO