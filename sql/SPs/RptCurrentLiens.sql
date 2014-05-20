IF OBJECT_ID('RptCurrentLiens') IS NULL
	EXECUTE('CREATE PROCEDURE RptCurrentLiens AS SELECT 1')
GO

ALTER PROCEDURE RptCurrentLiens
	 @DateStart DATETIME,
	 @DateEnd DATETIME

AS
BEGIN

SELECT count(1) Loans,sum(L.Principal) Principal,T.Name LienCompany 
FROM Loan L JOIN LoanLien T ON L.LoanLienId = T.Id 
GROUP BY T.Name

END 