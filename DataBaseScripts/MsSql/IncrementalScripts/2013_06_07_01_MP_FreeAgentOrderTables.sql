IF OBJECT_ID ('dbo.MP_FreeAgentOrderItemBankTransaction') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentOrderItemBankTransaction
GO

IF OBJECT_ID ('dbo.MP_FreeAgentOrderItem') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentOrderItem
GO

IF OBJECT_ID ('dbo.MP_FreeAgentOrder') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentOrder
GO

CREATE TABLE dbo.MP_FreeAgentOrder
	(
	  Id                                         INT IDENTITY NOT NULL
	, CustomerMarketPlaceId                      INT NOT NULL
	, Created                                    DATETIME NOT NULL
	, CustomerMarketPlaceUpdatingHistoryRecordId INT
	, CONSTRAINT PK_MP_FreeAgentOrder PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_FreeAgentOrder_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)
GO

CREATE INDEX IX_MP_FreeAgentOrderCustomerMarketPlaceId
	ON dbo.MP_FreeAgentOrder (CustomerMarketPlaceId)
GO

CREATE TABLE dbo.MP_FreeAgentOrderItem
	(
     Id INT IDENTITY NOT NULL
    ,OrderId INT NOT NULL
    ,url NVARCHAR(250)
    ,contact NVARCHAR(250)
    ,dated_on DATETIME 
    ,due_on DATETIME 
    ,reference NVARCHAR(250) 
    ,currency NVARCHAR(10) 
    ,exchange_rate NUMERIC (18, 4) 
    ,net_value NUMERIC (18, 2) 
    ,total_value NUMERIC (18, 2) 
    ,paid_value NUMERIC (18, 2) 
    ,due_value NUMERIC (18, 2) 
    ,status NVARCHAR(250) 
    ,omit_header BIT 
    ,payment_terms_in_days INT 
    ,paid_on DATETIME 
	
    ,CONSTRAINT PK_MP_FreeAgentOrderItem PRIMARY KEY (Id)
   	,CONSTRAINT FK_MP_FreeAgentOrderItem_MP_FreeAgentOrder FOREIGN KEY (OrderId) REFERENCES dbo.MP_FreeAgentOrder (Id)
	)
GO

CREATE INDEX IX_MP_FreeAgentOrderItemOrderId
	ON dbo.MP_FreeAgentOrderItem (OrderId)
GO

IF NOT EXISTS (SELECT 1 FROM MP_MarketplaceType WHERE MP_MarketplaceType.Name='FreeAgent')
	INSERT INTO MP_MarketplaceType VALUES ('FreeAgent', '737691E8-5C77-48EF-B01B-7348E24094B6', 'FreeAgent', 1)
GO
