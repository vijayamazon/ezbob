IF OBJECT_ID (N'dbo.vw_LoansAmountByDay') IS NOT NULL
	DROP VIEW dbo.vw_LoansAmountByDay
GO

CREATE VIEW [dbo].[vw_LoansAmountByDay]
AS
SELECT ll.Date, sum(ll.LoanAmount) as Amount
  FROM (
	SELECT DATEADD(D, 0, DATEDIFF(D, 0, Date)) as Date, l.LoanAmount
	  FROM [dbo].[Loan] l
  ) ll
  group by Date

GO

