IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ZohoId' and Object_ID = Object_ID(N'CashRequests'))    
BEGIN
	ALTER TABLE CashRequests DROP COLUMN ZohoId
END
GO