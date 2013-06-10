IF OBJECT_ID ('dbo.MP_FreeAgentInvoiceItem') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentInvoiceItem
GO
IF OBJECT_ID ('dbo.MP_FreeAgentOrderItem') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentOrderItem
GO
IF OBJECT_ID ('dbo.MP_FreeAgentCompany') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentCompany
GO
IF OBJECT_ID ('dbo.MP_FreeAgentUsers') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentUsers
GO
IF OBJECT_ID ('dbo.MP_FreeAgentOrder') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentOrder
GO
IF OBJECT_ID ('dbo.MP_FreeAgentInvoice') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentInvoice
GO
IF OBJECT_ID ('dbo.MP_FreeAgentRequest') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentRequest
GO

CREATE TABLE dbo.MP_FreeAgentRequest
	(
	  Id                                         INT IDENTITY NOT NULL
	, CustomerMarketPlaceId                      INT NOT NULL
	, Created                                    DATETIME NOT NULL
	, CustomerMarketPlaceUpdatingHistoryRecordId INT
	, CONSTRAINT PK_MP_FreeAgentRequest PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_FreeAgentRequest_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)
GO

CREATE INDEX IX_MP_FreeAgentRequestCustomerMarketPlaceId
	ON dbo.MP_FreeAgentRequest (CustomerMarketPlaceId)
GO

CREATE TABLE dbo.MP_FreeAgentInvoice
	(
     Id INT IDENTITY NOT NULL
    ,RequestId INT NOT NULL
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
	
    ,CONSTRAINT PK_MP_FreeAgentInvoice PRIMARY KEY (Id)
   	,CONSTRAINT FK_MP_FreeAgentInvoice_MP_FreeAgentRequest FOREIGN KEY (RequestId) REFERENCES dbo.MP_FreeAgentRequest (Id)
	)
GO

CREATE INDEX IX_PK_MP_FreeAgentInvoiceRequestId
	ON dbo.MP_FreeAgentInvoice (RequestId)
GO


CREATE TABLE dbo.MP_FreeAgentCompany
	(
	  Id INT IDENTITY NOT NULL
	, RequestId INT NOT NULL
	, url NVARCHAR(250)
	, name NVARCHAR(250)
	, subdomain NVARCHAR(250)
	, type NVARCHAR(250)
	, currency NVARCHAR(250)
	, mileage_units NVARCHAR(250)
	, company_start_date DATETIME
	, freeagent_start_date DATETIME
	, first_accounting_year_end DATETIME
	, company_registration_number INT
	, sales_tax_registration_status NVARCHAR(250)
	, sales_tax_registration_number INT
	, CONSTRAINT PK_MP_FreeAgentCompany PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_FreeAgentCompany_MP_FreeAgentRequest FOREIGN KEY (RequestId) REFERENCES dbo.MP_FreeAgentRequest (Id)
	)
GO

CREATE INDEX IX_MP_FreeAgentCompanyRequestId
	ON dbo.MP_FreeAgentCompany (RequestId)
GO

CREATE TABLE dbo.MP_FreeAgentUsers
	(
	  Id INT IDENTITY NOT NULL
	, RequestId INT NOT NULL
	, url NVARCHAR(250)
	, first_name NVARCHAR(250)
	, last_name NVARCHAR(250)
	, email NVARCHAR(250)
	, role NVARCHAR(250)
	, permission_level INT
	, opening_mileage NUMERIC(18,2)
	, updated_at DATETIME
	, created_at DATETIME
	, CONSTRAINT PK_MP_FreeAgentUsers PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_FreeAgentUsers_MP_FreeAgentRequest FOREIGN KEY (RequestId) REFERENCES dbo.MP_FreeAgentRequest (Id)
	)
GO

CREATE INDEX IX_MP_FreeAgentUsersRequestId
	ON dbo.MP_FreeAgentUsers (RequestId)
GO
