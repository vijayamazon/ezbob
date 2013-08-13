
IF OBJECT_ID ('dbo.MP_PayPalTransactionItem2') IS NOT NULL
	DROP TABLE dbo.MP_PayPalTransactionItem2
GO

CREATE TABLE dbo.MP_PayPalTransactionItem2
	(
	  Id                  INT IDENTITY NOT NULL
	, TransactionId       INT NOT NULL
	, Created             DATETIME
	, CurrencyId          INT
	, FeeAmount           FLOAT
	, GrossAmount         FLOAT
	, NetAmount           FLOAT
	, TimeZone            NVARCHAR (128)
	, Type                NVARCHAR (128)
	, Status              NVARCHAR (128)
	, PayPalTransactionId NVARCHAR (128)
	, CONSTRAINT PK_MP_TransactionItem2 PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_TransactionItem2_MP_Transaction FOREIGN KEY (TransactionId) REFERENCES dbo.MP_PayPalTransaction (Id)
	, CONSTRAINT FK_MP_TransactionItem2_MP_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.MP_Currency (Id)
	)
GO

CREATE INDEX IX_MP_PayPalTransactionItem2_Type
	ON dbo.MP_PayPalTransactionItem2 (TransactionId)
	WITH (FILLFACTOR = 90)
GO

CREATE INDEX MP_PayPalTransactionItem2_TI
	ON dbo.MP_PayPalTransactionItem2 (Created, Type, Status)
GO

