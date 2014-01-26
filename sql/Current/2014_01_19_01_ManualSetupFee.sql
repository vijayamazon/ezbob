IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ManualSetupFeeAmount' and Object_ID = Object_ID(N'CashRequests'))
BEGIN 
ALTER TABLE CashRequests ADD ManualSetupFeeAmount INT 
ALTER TABLE CashRequests ADD ManualSetupFeePercent DECIMAL(18)
END 

GO 
