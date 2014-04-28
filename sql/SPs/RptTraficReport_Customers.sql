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

SELECT count(DISTINCT c.Id) Customers, 
       sum(CASE WHEN c.WizardStep=4 THEN 1 ELSE 0 END) AS Applications, 
       sum(CASE WHEN c.NumApproves > 0 THEN 1 ELSE 0 END) AS NumOfApproved,
       sum(CASE WHEN c.NumRejects > 0 AND c.NumApproves=0 THEN 1 ELSE 0 END) AS NumOfRejected, 
       count(l.Id) NumOfLoans, 
       sum(l.LoanAmount) LoanAmount, c.ReferenceSource, c.GoogleCookie 
FROM Customer c LEFT JOIN Loan l ON l.CustomerId=c.Id 
WHERE c.GreetingMailSentDate>=@DateStart AND c.GreetingMailSentDate<@DateEnd AND c.IsTest=0
GROUP BY c.ReferenceSource, c.GoogleCookie

END
GO