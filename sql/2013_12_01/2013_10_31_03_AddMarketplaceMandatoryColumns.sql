IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MandatoryOnline' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType ADD MandatoryOnline BIT
END 
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MandatoryOffline' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType ADD MandatoryOffline BIT
END 
GO

IF EXISTS (SELECT 1 FROM MP_MarketplaceType WHERE MandatoryOnline IS NULL)
BEGIN
	UPDATE MP_MarketplaceType SET MandatoryOnline = 0, MandatoryOffline = 0
	UPDATE MP_MarketplaceType SET MandatoryOffline = 1 WHERE Name = 'HMRC'
END
GO
