
IF OBJECT_ID ('dbo.MP_FreeAgentCompany') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentCompany
GO

CREATE TABLE dbo.MP_FreeAgentCompany
	(
	  Id INT IDENTITY NOT NULL
	, OrderId INT NOT NULL
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
	, CONSTRAINT FK_MP_FreeAgentCompany_MP_FreeAgentOrder FOREIGN KEY (OrderId) REFERENCES dbo.MP_FreeAgentOrder (Id)
	)
GO

CREATE INDEX IX_MP_FreeAgentCompanyOrderId
	ON dbo.MP_FreeAgentCompany (OrderId)
GO

IF OBJECT_ID ('dbo.MP_FreeAgentUsers') IS NOT NULL
	DROP TABLE dbo.MP_FreeAgentUsers
GO

CREATE TABLE dbo.MP_FreeAgentUsers
	(
	  Id INT IDENTITY NOT NULL
	, OrderId INT NOT NULL
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
	, CONSTRAINT FK_MP_FreeAgentUsers_MP_FreeAgentOrder FOREIGN KEY (OrderId) REFERENCES dbo.MP_FreeAgentOrder (Id)
	)
GO

CREATE INDEX IX_MP_FreeAgentUsersOrderId
	ON dbo.MP_FreeAgentUsers (OrderId)
GO