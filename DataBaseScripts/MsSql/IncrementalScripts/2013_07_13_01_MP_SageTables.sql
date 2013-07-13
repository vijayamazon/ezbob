IF OBJECT_ID ('dbo.MP_SageInvoiceItem') IS NOT NULL
	DROP TABLE dbo.MP_SageInvoiceItem
GO

IF OBJECT_ID ('dbo.MP_SageInvoice') IS NOT NULL
	DROP TABLE dbo.MP_SageInvoice
GO

IF OBJECT_ID ('dbo.MP_SageRequest') IS NOT NULL
	DROP TABLE dbo.MP_SageRequest
GO

CREATE TABLE dbo.MP_SageRequest
	(
	  Id                                         INT IDENTITY NOT NULL
	, CustomerMarketPlaceId                      INT NOT NULL
	, Created                                    DATETIME NOT NULL
	, CustomerMarketPlaceUpdatingHistoryRecordId INT
	, CONSTRAINT PK_MP_SageRequest PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_SageRequest_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)
GO

CREATE INDEX IX_MP_SageRequestCustomerMarketPlaceId
	ON dbo.MP_SageRequest (CustomerMarketPlaceId)
GO

CREATE TABLE dbo.MP_SageInvoice
	(
     Id INT IDENTITY NOT NULL
    ,RequestId INT NOT NULL
    ,SageId INT NOT NULL
    ,invoice_number NVARCHAR(250)
    ,StatusId INT
    ,due_date DATETIME 
    ,date DATETIME 
    ,void_reason NVARCHAR(250) 
    ,outstanding_amount NUMERIC (18, 2) 
    ,total_net_amount NUMERIC (18, 2) 
    ,total_tax_amount NUMERIC (18, 2) 
    ,tax_scheme_period_id INT
    ,carriage NUMERIC (18, 2) 
    ,CarriageTaxCodeId INT
    ,carriage_tax_rate_percentage NUMERIC (18, 2)  
    ,ContactId INT 
    ,contact_name NVARCHAR(250)
    ,main_address NVARCHAR(250)
    ,delivery_address NVARCHAR(250)
    ,delivery_address_same_as_main BIT
    ,reference NVARCHAR(250)
    ,notes NVARCHAR(250)
    ,terms_and_conditions NVARCHAR(250)
    ,lock_version INT
	
    ,CONSTRAINT PK_MP_SageInvoice PRIMARY KEY (Id)
   	,CONSTRAINT FK_MP_SageInvoice_MP_SageRequest FOREIGN KEY (RequestId) REFERENCES dbo.MP_SageRequest (Id)
	)
GO

CREATE INDEX IX_MP_SageInvoiceRequestId
	ON dbo.MP_SageInvoice (RequestId)
GO


CREATE TABLE dbo.MP_SageInvoiceItem
	(
     Id INT IDENTITY NOT NULL
    ,InvoiceId INT NOT NULL
    ,SageId INT NOT NULL	
    ,description NVARCHAR(250)
    ,quantity NUMERIC (18, 2) 
    ,unit_price NUMERIC (18, 2)  
    ,net_amount NUMERIC (18, 2)  
    ,tax_amount NUMERIC (18, 2)  
    ,TaxCodeId INT 
    ,tax_rate_percentage NUMERIC (18, 2) 
    ,unit_price_includes_tax BIT 
    ,LedgerAccountId INT
    ,product_code NVARCHAR(250) 
    ,ProductId INT
    ,ServiceKey INT
    ,lock_version INT
	
    ,CONSTRAINT PK_MP_SageInvoiceItem PRIMARY KEY (Id)
   	,CONSTRAINT FK_MP_SageInvoiceItem_MP_SageInvoice FOREIGN KEY (InvoiceId) REFERENCES dbo.MP_SageInvoice (Id)
	)
GO

CREATE INDEX IX_MP_SageInvoiceItemInvoiceId
	ON dbo.MP_SageInvoiceItem (InvoiceId)
GO
IF NOT EXISTS (SELECT 1 FROM MP_MarketplaceType WHERE MP_MarketplaceType.Name='Sage')
	INSERT INTO MP_MarketplaceType VALUES ('Sage', '4966BB57-0146-4E3D-AA24-F092D90B7923', 'Sage', 1)
GO
