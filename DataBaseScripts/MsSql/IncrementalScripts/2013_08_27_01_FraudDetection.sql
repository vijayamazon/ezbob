IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Concurrence' and Object_ID = Object_ID(N'FraudDetection'))    
BEGIN
	alter table FraudDetection add Concurrence NVARCHAR(250)
END
go