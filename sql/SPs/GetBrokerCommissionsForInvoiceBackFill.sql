IF OBJECT_ID('GetBrokerCommissionsForInvoiceBackFill') IS NULL 
	EXECUTE('CREATE PROCEDURE GetBrokerCommissionsForInvoiceBackFill AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetBrokerCommissionsForInvoiceBackFill
AS
BEGIN
	SELECT 
		lb.LoanBrokerCommissionID,
		lb.BrokerID,
		lb.CommissionAmount,
		lb.PaidDate,
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
			(lb.InvoiceSent IS NULL OR lb.InvoiceSent = 0)
		AND
			lb.PaidDate IS NOT NULL 
		AND 
			lb.Status='Done'
END
GO