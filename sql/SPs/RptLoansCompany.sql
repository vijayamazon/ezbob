
IF OBJECT_ID('RptLoansCompany') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansCompany AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoansCompany
(@DateStart DATETIME,	@DateEnd DATETIME)
AS
BEGIN

	SELECT c.Id CustomerId, c.Fullname, c.Name Email, co.CompanyName, co.ExperianCompanyName, co.ExperianRefNum, l.RefNum LoanRefNum 
	FROM Loan l 
	INNER JOIN Customer c ON l.CustomerId = c.Id
	LEFT JOIN Company co ON c.CompanyId = co.Id
	WHERE l.DateClosed IS NULL AND
	c.IsTest = 0

END 
GO
