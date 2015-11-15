IF OBJECT_ID('NL_GetCuredLoansForCollection') IS NOT NULL
	DROP PROCEDURE NL_GetCuredLoansForCollection
GO

CREATE PROCEDURE NL_GetCuredLoansForCollection
AS
BEGIN
SELECT c.Id CustomerID, l.LoanID LoanID, loanHistoryGrouBy.maxLoanHistoryID asLoanHistoryID
FROM Customer c 
	INNER JOIN vw_NL_LoansCustomer v ON v.CustomerID = c.Id
	INNER JOIN NL_Loans l ON v.loanID = l.LoanID	
	INNER JOIN(SELECT LoanID, MAX(LoanHistoryID) AS maxLoanHistoryID 
			   FROM NL_LoanHistory 
			   GROUP BY LoanID) loanHistoryGrouBy
	on loanHistoryGrouBy.LoanID = l.LoanID  
	INNER JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus
WHERE cs.IsAutomaticStatus = 1
	AND cs.Name<>'Enabled' 
	AND NOT EXISTS (SELECT 1 FROM NL_Loans l2 
				INNER JOIN vw_NL_LoansCustomer v2 ON v.CustomerID = c.Id
				INNER JOIN Customer c2 ON v2.CustomerID = c2.Id
				INNER JOIN NL_LoanStatuses ls on l2.LoanStatusID = ls.LoanStatusID
	 				WHERE ls.LoanStatus ='Late' 
	 				AND c2.id = c.Id
	 			)
END