IF OBJECT_ID ('dbo.MP_SageSalesInvoiceItem') IS NOT NULL
	DROP TABLE dbo.MP_SageSalesInvoiceItem
GO

CREATE TABLE dbo.MP_SageSalesInvoiceItem
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
    ,ServiceId INT
    ,lock_version INT
	
    ,CONSTRAINT PK_MP_SageSalesInvoiceItem PRIMARY KEY (Id)
   	,CONSTRAINT FK_MP_SageSalesInvoiceItem_MP_SageSalesInvoice FOREIGN KEY (InvoiceId) REFERENCES dbo.MP_SageSalesInvoice (Id)
	)
GO

CREATE INDEX IX_MP_SageSalesInvoiceItemInvoiceId
	ON dbo.MP_SageSalesInvoiceItem (InvoiceId)
GO

