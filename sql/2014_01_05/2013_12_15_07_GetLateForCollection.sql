IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLateForCollection]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLateForCollection]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLateForCollection] 
AS
BEGIN
	SELECT 
		convert(DATE, ls.Date) AS DATE, 
		ls.AmountDue, 
		ls.LoanId, 
		ls.Interest, 
		c.Name AS email, 
		c.FirstName, 
		ls.Id AS LoanScheduleId, 
		l.MaxDelinquencyDays, 
		l.RefNum, 
		ls.Delinquency AS InstallmentDelinquency, 
		ls.CustomInstallmentDate 
	FROM 
		loanschedule ls 
		LEFT JOIN Loan l ON 
			l.Id = ls.LoanId
		LEFT JOIN customer c ON 
			l.CustomerId = c.Id 
		JOIN CustomerStatuses cs ON 
			cs.Id = c.CollectionStatus AND 
			cs.IsEnabled = 1
	WHERE 
		ls.Status = 'Late' AND 
		l.Id IS NOT NULL
END
GO
