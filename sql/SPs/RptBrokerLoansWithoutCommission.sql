
IF OBJECT_ID('RptBrokerLoansWithoutCommission') IS NULL
	EXECUTE ('CREATE PROCEDURE RptBrokerLoansWithoutCommission AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptBrokerLoansWithoutCommission (@DateStart DATETIME,@DateEnd   DATETIME)
AS
BEGIN

SELECT l.Id, l.CustomerId, c.BrokerID, l.[Date], l.LoanAmount, l.SetupFee, cr.IdUnderwriter, isnull(cr.BrokerSetupFeePercent,0) BrokerSetupFeePercent, isnull(cr.ManualSetupFeePercent,0) ManualSetupFeePercent
FROM Loan l INNER JOIN Customer c ON l.CustomerId=c.Id
INNER JOIN CashRequests cr ON cr.Id=l.RequestCashId
LEFT JOIN LoanBrokerCommission lb ON lb.LoanID = l.Id
WHERE l.[Date] > @DateStart AND l.[Date] < @DateEnd AND c.BrokerID IS NOT NULL AND lb.LoanBrokerCommissionID IS NULL AND c.BrokerID NOT IN (23743, 23423) 
AND l.[Date] > '2015-05-05'

END
GO