IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'MonthlyStatusEnabled' and Object_ID = Object_ID(N'Customer'))
BEGIN 
	ALTER TABLE Customer ADD MonthlyStatusEnabled BIT
END 
GO



