IF OBJECT_ID ('dbo.MP_AmazonMarketplaceType') IS NOT NULL
	DROP TABLE dbo.MP_AmazonMarketplaceType

CREATE TABLE dbo.MP_AmazonMarketplaceType
	(
	  Id                           INT IDENTITY NOT NULL
	, MarketplaceId                NVARCHAR(20) NOT NULL
	, Country					   NVARCHAR(50)
	, Domain					   NVARCHAR(50)
	, CONSTRAINT PK_MP_AmazonMarketplaceType PRIMARY KEY (Id)
	)
INSERT INTO MP_AmazonMarketplaceType (MarketplaceId, Country, Domain) VALUES ( 'A1F83G8C2ARO7P', 'United Kingdom', 'www.amazon.co.uk')
INSERT INTO MP_AmazonMarketplaceType (MarketplaceId, Country, Domain) VALUES ( 'A13V1IB3VIYZZH', 'France', 'www.amazon.fr')
INSERT INTO MP_AmazonMarketplaceType (MarketplaceId, Country, Domain) VALUES ( 'A1PA6795UKMFR9', 'Germany', 'www.amazon.de')
INSERT INTO MP_AmazonMarketplaceType (MarketplaceId, Country, Domain) VALUES ( 'APJ6JRA9NG5V4', 'Italy', 'www.amazon.it')
INSERT INTO MP_AmazonMarketplaceType (MarketplaceId, Country, Domain) VALUES ( 'A1RKKUPIHCS9HS', 'Spain', 'www.amazon.es')


