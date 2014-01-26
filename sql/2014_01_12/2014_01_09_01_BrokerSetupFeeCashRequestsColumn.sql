IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'UseBrokerSetupFee' and Object_ID = Object_ID(N'CashRequests'))
BEGIN 
ALTER TABLE CashRequests ADD UseBrokerSetupFee BIT NOT NULL DEFAULT(0)
END 
