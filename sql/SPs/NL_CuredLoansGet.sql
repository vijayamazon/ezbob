IF OBJECT_ID('NL_CuredLoansGet') IS  NULL
	EXECUTE('CREATE PROCEDURE NL_CuredLoansGet AS SELECT 1')
GO


ALTER PROCEDURE [dbo].[NL_CuredLoansGet]
AS
BEGIN
SELECT c.Id CustomerID, l.LoanID LoanID, l.OldLoanID, lastHistory.LoanHistoryID as LoanHistoryID
FROM Customer c 
	JOIN vw_NL_LoansCustomer v ON v.CustomerID = c.Id JOIN NL_Loans l ON v.loanID = l.LoanID	
	JOIN (SELECT LoanID, MAX(LoanHistoryID) AS LoanHistoryID 
			   FROM NL_LoanHistory 
			   GROUP BY LoanID) lastHistory
	on lastHistory.LoanID = l.LoanID  
	JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus
WHERE cs.IsAutomaticStatus = 1 AND cs.Name<>'Enabled' 
	AND NOT EXISTS (SELECT 1 FROM NL_Loans l2 
				JOIN vw_NL_LoansCustomer v2 ON v2.LoanID = l2.LoanID
				JOIN Customer c2 ON v2.CustomerID = c2.Id AND c2.Id = c.Id
				JOIN NL_LoanStatuses ls2 on l2.LoanStatusID = ls2.LoanStatusID
	 			WHERE ls2.LoanStatus ='Late')
END