IF OBJECT_ID (N'dbo.GetLoanLatePaymentsGrouped') IS NOT NULL
	DROP FUNCTION dbo.GetLoanLatePaymentsGrouped
GO

CREATE FUNCTION [dbo].[GetLoanLatePaymentsGrouped]
(	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT dbo.Loan.RequestCashId, SUM(lp.LatePaymentsAmount) LatePaymentsAmount FROM 
	dbo.Loan LEFT OUTER JOIN GetLatePaymentsGrouped() as lp on lp.LoanId = dbo.Loan.Id
	Group by dbo.Loan.RequestCashId
)

GO

