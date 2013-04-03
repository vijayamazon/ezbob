IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_LoansAmountByDay]'))
DROP VIEW [dbo].[vw_LoansAmountByDay]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_LoansAmountByDay]
as
SELECT ll.Date, sum(ll.LoanAmount) as Amount
  FROM (
	SELECT DATEADD(D, 0, DATEDIFF(D, 0, Date)) as Date, l.LoanAmount
	  FROM [dbo].[Loan] l
  ) ll
  group by Date
GO
