IF OBJECT_ID('UpdateBrokerCommissionsInvoiceBackFill') IS NULL 
	EXECUTE('CREATE PROCEDURE UpdateBrokerCommissionsInvoiceBackFill AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateBrokerCommissionsInvoiceBackFill
	(@LoanBrokerCommissionID INT)
AS
BEGIN
	UPDATE LoanBrokerCommission SET InvoiceSent = 1 WHERE LoanBrokerCommissionID = @LoanBrokerCommissionID
END
GO