IF OBJECT_ID ('dbo.MP_FreeAgentExpense') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentExpense
GO

IF OBJECT_ID ('dbo.MP_FreeAgentExpenseCategory') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentExpenseCategory
GO

CREATE TABLE dbo.MP_FreeAgentExpenseCategory
	(
		Id INT IDENTITY NOT NULL,
		url NVARCHAR(250),
		description NVARCHAR(250),
		nominal_code NVARCHAR(250),
		allowable_for_tax BIT,
		tax_reporting_name NVARCHAR(250),
		auto_sales_tax_rate NVARCHAR(250)
		
	  CONSTRAINT PK_MP_FreeAgentExpenseCategory PRIMARY KEY (Id)
	, CONSTRAINT IX_MP_FreeAgentExpenseCategory_url UNIQUE (url)
	)
GO

CREATE INDEX IX_MP_FreeAgentExpenseCategoryurl
	ON dbo.MP_FreeAgentExpenseCategory (url)
GO

CREATE TABLE dbo.MP_FreeAgentExpense
	(
	  Id INT IDENTITY NOT NULL,
	  RequestId INT NOT NULL,
	  CategoryId INT NOT NULL,
	  url NVARCHAR(250),
	  username NVARCHAR(250),
	  category NVARCHAR(250),
	  dated_on DATETIME,
	  currency NVARCHAR(10),
	  gross_value NUMERIC (18, 2),
	  native_gross_value NUMERIC (18, 2),
	  sales_tax_rate NUMERIC (18, 2),
	  sales_tax_value NUMERIC (18, 2),
	  native_sales_tax_value NUMERIC (18, 2),
	  description NVARCHAR(250),
	  manual_sales_tax_amount NUMERIC (18, 2),
	  updated_at DATETIME,
	  created_at DATETIME,
	  attachment_url NVARCHAR(250),
	  attachment_content_src NVARCHAR(1000),
	  attachment_content_type NVARCHAR(250),
	  attachment_file_name NVARCHAR(250),
	  attachment_file_size INT,
	  attachment_description NVARCHAR(250)
	  	  
	  CONSTRAINT PK_MP_FreeAgentExpense PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_FreeAgentExpense_MP_FreeAgentRequest FOREIGN KEY (RequestId) REFERENCES dbo.MP_FreeAgentRequest (Id)
	, CONSTRAINT FK_MP_FreeAgentExpense_MP_FreeAgentExpenseCategory FOREIGN KEY (CategoryId) REFERENCES dbo.MP_FreeAgentExpenseCategory (Id)
	)
GO

CREATE INDEX IX_MP_FreeAgentExpenseRequestId
	ON dbo.MP_FreeAgentExpense (RequestId)
GO

CREATE INDEX IX_MP_FreeAgentExpenseCategoryId
	ON dbo.MP_FreeAgentExpense (CategoryId)
GO