IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'siteAccountStatus' and Object_ID = Object_ID(N'MP_YodleeOrderItem'))
BEGIN 
	ALTER TABLE MP_YodleeOrderItem 
	ADD siteAccountStatus NVARCHAR(50)
END 
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'accountClassification' and Object_ID = Object_ID(N'MP_YodleeOrderItem'))
BEGIN
	ALTER TABLE MP_YodleeOrderItem 
	ADD accountClassification NVARCHAR(50)
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'itemAccountId' and Object_ID = Object_ID(N'MP_YodleeOrderItem'))
BEGIN
	ALTER TABLE MP_YodleeOrderItem 
	ADD itemAccountId BIGINT
END
GO


