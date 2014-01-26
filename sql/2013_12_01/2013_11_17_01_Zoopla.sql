IF OBJECT_ID('PK_CustomerAddress', 'PK') IS NULL 
	ALTER TABLE CustomerAddress ADD CONSTRAINT PK_CustomerAddress PRIMARY KEY (addressId)
	
GO	

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Zoopla]') AND type IN (N'U'))
BEGIN
	CREATE TABLE Zoopla (
		Id INT IDENTITY(1,1) NOT NULL 
	  , CustomerAddressId INT NOT NULL
	  , AreaName NVARCHAR(10)
	  ,	AverageSoldPrice1Year INT 
	  ,	AverageSoldPrice3Year INT 
	  ,	AverageSoldPrice5Year INT 
	  ,	AverageSoldPrice7Year INT 
	  , NumerOfSales1Year INT 
	  , NumerOfSales3Year INT 
	  , NumerOfSales5Year INT 
	  , NumerOfSales7Year INT 
      , TurnOver DECIMAL(18)
      , PricesUrl NVARCHAR(100)
	  , AverageValuesGraphUrl NVARCHAR(100)
	  , ValueRangesGraphUrl NVARCHAR(100)
	  , ValueTrendGraphUrl NVARCHAR(100)
	  , HomeValuesGraphUrl NVARCHAR(100)
	  , CONSTRAINT PK_Zoopla PRIMARY KEY (Id)
	  , CONSTRAINT FK_Zoopla_CustomerAddress FOREIGN KEY (CustomerAddressId) REFERENCES CustomerAddress (addressId)
	)
	
	CREATE INDEX IX_ZooplaCustomerAddress_ID
	ON dbo.Zoopla (CustomerAddressId)


END

GO


