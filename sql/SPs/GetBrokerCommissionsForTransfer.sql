IF OBJECT_ID('GetBrokerCommissionsForTransfer') IS NULL
BEGIN
   EXECUTE('CREATE PROCEDURE GetBrokerCommissionsForTransfer AS SELECT 1')
END 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE GetBrokerCommissionsForTransfer
AS
BEGIN
	SELECT 
		lb.LoanBrokerCommissionID,
		lb.BrokerID,
		lb.CommissionAmount,
		b.ContactName,
		ci.SortCode,
		ci.BankAccount
	FROM 
		LoanBrokerCommission lb 
		INNER JOIN Broker b ON b.BrokerID = lb.BrokerID
		INNER JOIN CardInfo ci ON ci.BrokerID = lb.CardInfoID
	WHERE 
		lb.PaidDate IS NULL
		AND 
		lb.CardInfoID IS NOT NULL
END
GO