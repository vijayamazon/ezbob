IF OBJECT_ID('NL_GetNumberOfActiveLoans') IS NOT NULL
	DROP PROCEDURE NL_GetNumberOfActiveLoans GO

CREATE PROCEDURE [dbo].NL_GetNumberOfActiveLoans @CustomerID INT AS 
BEGIN
	SELECT COUNT(*)
	FROM NL_loans l
	JOIN vw_NL_LoansCustomer v ON v.LoanID = l.LoanID
	JOIN NL_LoanStatuses lst ON lst.LoanStatusID = l.LoanStatusID
	WHERE v.CustomerId = @CustomerID
	  AND lst.LoanStatus != 'PaidOff' 
END