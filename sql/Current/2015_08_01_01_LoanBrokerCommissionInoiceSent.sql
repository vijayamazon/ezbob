IF NOT EXISTS (SELECT * FROM syscolumns WHERE Name='InvoiceSent' AND id = object_id('LoanBrokerCommission'))
BEGIN
	ALTER TABLE LoanBrokerCommission ADD InvoiceSent BIT 
END
GO
