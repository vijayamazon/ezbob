ALTER TABLE dbo.MP_CustomerMarketPlace ADD
	TokenExpired int NOT NULL CONSTRAINT DF_MP_CustomerMarketPlace_TokenExpired DEFAULT 0