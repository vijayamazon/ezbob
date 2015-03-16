IF OBJECT_ID ('dbo.MP_MarketplaceGroup') IS NULL
BEGIN 	
	CREATE TABLE dbo.MP_MarketplaceGroup
		(
		  Id                     INT IDENTITY NOT NULL
		, Name                   NVARCHAR (50)
		, CONSTRAINT PK_MP_MarketplaceGroup PRIMARY KEY (Id) 
		)
		
	INSERT INTO MP_MarketplaceGroup	(Name) VALUES ('Accounting')
	INSERT INTO MP_MarketplaceGroup	(Name) VALUES ('Shop')
	INSERT INTO MP_MarketplaceGroup	(Name) VALUES ('Bank')
	INSERT INTO MP_MarketplaceGroup	(Name) VALUES ('Other')
END
GO  
