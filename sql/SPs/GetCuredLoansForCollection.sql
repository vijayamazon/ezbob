IF OBJECT_ID('GetCuredLoansForCollection') IS NULL
	EXECUTE('CREATE PROCEDURE GetCuredLoansForCollection AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCuredLoansForCollection
AS
BEGIN
	SELECT c.Id CustomerID, l.Id LoanID
	FROM Customer c
	 INNER JOIN Loan l ON c.Id = l.CustomerId 
	 INNER JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus
	WHERE cs.IsAutomaticStatus = 1
	 AND cs.Name<>'Enabled' 
	 AND NOT EXISTS (SELECT 1 
	 				 FROM Loan l2 
	 				 WHERE l2.Status='Late' 
	 				 AND l2.CustomerId = c.Id
	 				)  
END
GO