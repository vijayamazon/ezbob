IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetPacnetTransactions') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetPacnetTransactions
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetPacnetTransactions 
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
				lt.Status IS NULL 
			) AND 
			lt.Type = 'PacnetTransaction' 
END
GO
