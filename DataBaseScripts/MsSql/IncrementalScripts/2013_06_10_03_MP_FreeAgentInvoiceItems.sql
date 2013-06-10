IF OBJECT_ID ('dbo.MP_FreeAgentInvoiceItem') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentInvoiceItem
GO

CREATE TABLE dbo.MP_FreeAgentInvoiceItem
	(
	  Id INT IDENTITY NOT NULL,
	  InvoiceId INT NOT NULL,
	  url NVARCHAR(250),
	  position INT,
	  description NVARCHAR(250),
	  item_type NVARCHAR(250),
	  price NUMERIC (18, 2),
	  quantity NUMERIC (18, 2),
	  category NVARCHAR(250)
	  
	  CONSTRAINT PK_MP_FreeAgentInvoiceItem PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_FreeAgentInvoiceItem_MP_FreeAgentInvoice FOREIGN KEY (InvoiceId) REFERENCES dbo.MP_FreeAgentInvoice (Id)
	)
GO

CREATE INDEX IX_MP_FreeAgentInvoiceItemInvoiceId
	ON dbo.MP_FreeAgentInvoiceItem (InvoiceId)
GO

