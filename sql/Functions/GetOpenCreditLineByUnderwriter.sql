IF OBJECT_ID (N'dbo.GetOpenCreditLineByUnderwriter') IS NOT NULL
	DROP FUNCTION dbo.GetOpenCreditLineByUnderwriter
GO

CREATE FUNCTION [dbo].[GetOpenCreditLineByUnderwriter]
(	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT IdUnderwriter, SUM(SystemCalculatedSum - l.loanAmount) as OpenCreditLine from CashRequests 
LEFT OUTER JOIN 
(SELECT RequestCashId, SUM(LoanAmount) as loanAmount FROM Loan
WHERE RequestCashId IS NOT NULL
Group by RequestCashId) as l ON CashRequests.Id = l.RequestCashId
WHERE UnderwriterDecisionDate > (GetDate() - 24)
Group BY IdUnderwriter
)

GO

