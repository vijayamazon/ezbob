IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptTraficReport_Customers]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptTraficReport_Customers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RptTraficReport_Customers] 
	(@DateStart DATE, @DateEnd DATE)
AS
BEGIN

SELECT count(c.Id) Customers, count(l.Id) NumOfLoans, sum(l.LoanAmount) LoanAmount, c.ReferenceSource, c.GoogleCookie 
FROM Customer c LEFT JOIN Loan l ON l.CustomerId=c.Id 
WHERE c.GreetingMailSentDate>=@DateStart AND c.GreetingMailSentDate<@DateEnd 
GROUP BY c.ReferenceSource, c.GoogleCookie

END