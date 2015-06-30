IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPacnetTransactions]') AND TYPE IN (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE GetPacnetTransactions AS SELECT 1')
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetPacnetTransactions]
AS
BEGIN
	SELECT  
		lt.TrackingNumber, 
		lt.Description AS allDescriptions, 
		l.CustomerId
	FROM 
		LoanTransaction lt 
		JOIN Loan l ON 
			l.Id = lt.LoanId AND 
			(
				lt.Status = 'InProgress' OR 
				lt.Status IS NULL OR
				(lt.Description = 'Raven server communication error.' AND lt.Status='Error')
			) AND 
			lt.Type = 'PacnetTransaction'
END
GO
