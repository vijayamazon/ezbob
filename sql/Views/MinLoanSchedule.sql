IF OBJECT_ID (N'dbo.MinLoanSchedule') IS NOT NULL
	DROP VIEW dbo.MinLoanSchedule
GO

CREATE VIEW [dbo].[MinLoanSchedule]
AS
select MIN(ls.date) as lsdate, l.Id
FROM LoanSchedule ls 
  LEFT OUTER JOIN dbo.Loan AS l ON l.Id = ls.LoanId
where ls.Status = 'Late'
GROUP BY l.Id

GO

