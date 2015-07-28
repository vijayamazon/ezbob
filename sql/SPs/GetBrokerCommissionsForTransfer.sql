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
		ci.BankAccount,
		c.Fullname CustomerName
	FROM 
		LoanBrokerCommission lb 
		INNER JOIN Broker b ON b.BrokerID = lb.BrokerID
		INNER JOIN CardInfo ci ON ci.Id = lb.CardInfoID
		INNER JOIN Loan l ON l.Id = lb.LoanID
		INNER JOIN Customer c ON c.Id = l.CustomerId
	WHERE 
		lb.PaidDate IS NULL
		AND 
		lb.CardInfoID IS NOT NULL
END

GO