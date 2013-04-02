IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[MinLoanSchedule]'))
DROP VIEW [dbo].[MinLoanSchedule]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view MinLoanSchedule
as 
select MIN(ls.date) as lsdate, l.Id
FROM LoanSchedule ls 
  LEFT OUTER JOIN dbo.Loan AS l ON l.Id = ls.LoanId
where ls.Status = 'Late'
GROUP BY l.Id
GO
