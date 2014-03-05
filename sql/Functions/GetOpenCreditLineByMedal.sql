IF OBJECT_ID (N'dbo.GetOpenCreditLineByMedal') IS NOT NULL
	DROP FUNCTION dbo.GetOpenCreditLineByMedal
GO

CREATE FUNCTION [dbo].[GetOpenCreditLineByMedal]
(	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT UPPER(MedalType) Medal, SUM(SystemCalculatedSum - l.loanAmount) as OpenCreditLine from CashRequests 
LEFT OUTER JOIN 
(SELECT RequestCashId, SUM(LoanAmount) as loanAmount FROM Loan
WHERE RequestCashId IS NOT NULL
Group by RequestCashId) as l ON CashRequests.Id = l.RequestCashId
WHERE UnderwriterDecisionDate > (GetDate() - 24)
Group BY UPPER(MedalType)
)

GO

