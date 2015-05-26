IF OBJECT_ID('RptBrokersPendingCommission') IS NULL
                EXECUTE('CREATE PROCEDURE RptBrokersPendingCommission AS SELECT 1')
GO
 
ALTER PROCEDURE RptBrokersPendingCommission
@DateStart DATETIME,
@DateEnd DATETIME
AS 
BEGIN
SELECT b.BrokerID, b.ContactEmail, b.ContactName, CASE WHEN lc.CardInfoID IS NULL THEN 'No bank' ELSE 'Added a bank' END HasBank, lc.CommissionAmount, lc.CreateDate, lc.PaidDate, lc.Status
FROM LoanBrokerCommission lc INNER JOIN Broker b ON b.BrokerID = lc.BrokerID
WHERE lc.Status<>'Done' OR lc.Status IS NULL AND b.IsTest=0
END 
GO