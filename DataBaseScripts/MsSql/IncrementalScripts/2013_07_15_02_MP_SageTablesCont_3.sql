IF OBJECT_ID ('dbo.MP_SageExpenditure') IS NOT NULL
	DROP TABLE dbo.MP_SageExpenditure
GO

CREATE TABLE dbo.MP_SageExpenditure
	(
     Id INT IDENTITY NOT NULL
    ,RequestId INT NOT NULL
    ,SageId INT NOT NULL
    ,date DATETIME 
    ,invoice_date DATETIME 
    ,amount NUMERIC (18, 2) 
    ,tax_amount NUMERIC (18, 2) 
    ,gross_amount NUMERIC (18, 2) 
    ,tax_percentage_rate NUMERIC (18, 2)
	,TaxCodeId INT	
    ,tax_scheme_period_id INT
    ,reference NVARCHAR(250)
    ,ContactId INT 
    ,SourceId INT 
    ,DestinationId INT 
    ,PaymentMethodId INT 
    ,voided BIT
    ,lock_version INT
	
    ,CONSTRAINT PK_MP_SageExpenditure PRIMARY KEY (Id)
   	,CONSTRAINT FK_MP_SageExpenditure_MP_SageRequest FOREIGN KEY (RequestId) REFERENCES dbo.MP_SageRequest (Id)
	)
GO

CREATE INDEX IX_MP_SageExpenditureRequestId
	ON dbo.MP_SageExpenditure (RequestId)
GO

