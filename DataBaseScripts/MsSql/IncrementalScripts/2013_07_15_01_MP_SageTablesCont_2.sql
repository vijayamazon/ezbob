IF OBJECT_ID ('dbo.MP_SagePurchaseInvoiceItem') IS NOT NULL
	DROP TABLE dbo.MP_SagePurchaseInvoiceItem
GO
IF OBJECT_ID ('dbo.MP_SagePurchaseInvoice') IS NOT NULL
	DROP TABLE dbo.MP_SagePurchaseInvoice
GO

CREATE TABLE dbo.MP_SagePurchaseInvoice
	(
     Id INT IDENTITY NOT NULL
    ,RequestId INT NOT NULL
    ,SageId INT NOT NULL
    ,StatusId INT 
    ,due_date DATETIME 
    ,date DATETIME 
    ,void_reason NVARCHAR(250)	
    ,outstanding_amount NUMERIC (18, 2) 
    ,total_net_amount NUMERIC (18, 2) 
    ,total_tax_amount NUMERIC (18, 2) 
    ,tax_scheme_period_id INT	
    ,ContactId INT
    ,contact_name NVARCHAR(250)
    ,main_address NVARCHAR(250)
	,delivery_address NVARCHAR (250)
	,delivery_address_same_as_main BIT
	,reference NVARCHAR (250)
	,notes NVARCHAR (250)
	,terms_and_conditions NVARCHAR (250)
	,lock_version INT
	
    ,CONSTRAINT PK_MP_SagePurchaseInvoice PRIMARY KEY (Id)
   	,CONSTRAINT FK_MP_SagePurchaseInvoice_MP_SageRequest FOREIGN KEY (RequestId) REFERENCES dbo.MP_SageRequest (Id)
	)
GO

CREATE INDEX IX_MP_SagePurchaseInvoiceRequestId
	ON dbo.MP_SagePurchaseInvoice (RequestId)
GO

CREATE TABLE dbo.MP_SagePurchaseInvoiceItem
	(
     Id INT IDENTITY NOT NULL
    ,PurchaseInvoiceId INT NOT NULL
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
    ,ServiceId INT
    ,lock_version INT
	
    ,CONSTRAINT PK_MP_SagePurchaseInvoiceItem PRIMARY KEY (Id)
   	,CONSTRAINT FK_MP_SagePurchaseInvoiceItem_MP_SagePurchaseInvoice FOREIGN KEY (PurchaseInvoiceId) REFERENCES dbo.MP_SagePurchaseInvoice (Id)
	)
GO

CREATE INDEX IX_MP_SagePurchaseInvoiceItemPurchaseInvoiceId
	ON dbo.MP_SagePurchaseInvoiceItem (PurchaseInvoiceId)
GO
