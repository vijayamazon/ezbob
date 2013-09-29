IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'PromoCode' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer ADD PromoCode VARCHAR(30)
END
GO

