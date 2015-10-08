SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AlibabaContract') IS NULL
BEGIN
	CREATE TABLE [AlibabaContract] (
		[ContractId] INT IDENTITY(1, 1) NOT NULL,
		[RequestId] NVARCHAR(100) NULL,
		[ResponseId] NVARCHAR(100) NULL,
		[LoanId] BIGINT NULL,
		[OrderNumber] NVARCHAR(100) NULL,
		[ShippingMark] NVARCHAR(100) NULL,
		[TotalOrderAmount] INT NULL,
		[DeviationQuantityAllowed] INT NULL,
		[OrderAddtlDetails] NVARCHAR(100) NULL,
		[ShippingTerms] NVARCHAR(100) NULL,
		[ShippingDate] DATETIME NULL,
		[LoadingPort] NVARCHAR(100) NULL,
		[DestinationPort] NVARCHAR(100) NULL,
		[TACoveredAmount] INT NULL,
		[OrderDeposit] INT NULL,
		[OrderBalance] INT NULL,
		[OrderCurrency] NVARCHAR(100) NULL,
		[CommercialInvoice] VARBINARY(MAX) NULL,
		[BillOfLading] VARBINARY(MAX) NULL,
		[PackingList] VARBINARY(MAX) NULL,
		[Other] VARBINARY(MAX) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_AlibabaContract PRIMARY KEY ([ContractId])
	)
END
GO

IF OBJECT_ID('AlibabaContractItem') IS NULL
BEGIN
	CREATE TABLE [AlibabaContractItem] (
		[ItemId] INT IDENTITY(1, 1) NOT NULL,
		[ContractId] INT NOT NULL,
		[OrderProdNumber] BIGINT NULL,
		[ProductName] NVARCHAR(100) NULL,
		[ProductSpecs] NVARCHAR(100) NULL,
		[ProductQuantity] INT NULL,
		[ProductUnit] INT NULL,
		[ProductUnitPrice] INT NULL,
		[ProductTotalAmount] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_AlibabaContractItem PRIMARY KEY ([ItemId]),
		CONSTRAINT FK_AlibabaContractItem_ContractId FOREIGN KEY ([ContractId]) REFERENCES [AlibabaContract] ([ContractId])
	)
END
GO

IF OBJECT_ID('AlibabaSeller') IS NULL
BEGIN
	CREATE TABLE [AlibabaSeller] (
		[SellerId] INT IDENTITY(1, 1) NOT NULL,
		[ContractId] INT NULL,
		[BusinessName] NVARCHAR(100) NULL,
		[AliMemberId] NVARCHAR(100) NULL,
		[Street1] NVARCHAR(100) NULL,
		[Street2] NVARCHAR(100) NULL,
		[City] NVARCHAR(100) NULL,
		[State] NVARCHAR(100) NULL,
		[Country] NVARCHAR(100) NULL,
		[PostalCode] NVARCHAR(100) NULL,
		[AuthRepFname] NVARCHAR(100) NULL,
		[AuthRepLname] NVARCHAR(100) NULL,
		[Phone] NVARCHAR(100) NULL,
		[Fax] NVARCHAR(100) NULL,
		[Email] NVARCHAR(100) NULL,
		[GoldSupplierFlag] NVARCHAR(100) NULL,
		[TenureWithAlibaba] NVARCHAR(100) NULL,
		[BusinessStartDate] DATETIME NULL,
		[Size] INT NULL,
		[SuspiciousReportCountCounterfeitProduct] INT NULL,
		[SuspiciousReportCountRestrictedProhibitedProduct] INT NULL,
		[SuspiciousReportCountSuspiciousMember] INT NULL,
		[ResponseRate] INT NULL,
		[GoldMemberStartDate] DATETIME NULL,
		[QuotationPerformance] INT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_SellerId PRIMARY KEY ([SellerId]),
		CONSTRAINT FK_AlibabaSeller_ContractId FOREIGN KEY ([ContractId]) REFERENCES [AlibabaContract] ([ContractId]),
	)
END
GO

IF OBJECT_ID('AlibabaSellerBank') IS NULL
BEGIN
	CREATE TABLE [AlibabaSellerBank] (
		[BankId] INT IDENTITY(1, 1) NOT NULL,
		[SellerId] INT NULL,
		[BeneficiaryBank] NVARCHAR(100) NULL,
		[StreetAddr1] NVARCHAR(100) NULL,
		[StreetAddr2] NVARCHAR(100) NULL,		
		[City] NVARCHAR(100) NULL,
		[State] NVARCHAR(100) NULL,
		[Country] NVARCHAR(100) NULL,
		[PostalCode] NVARCHAR(100) NULL,
		[SwiftCode] NVARCHAR(100) NULL,
		[AccountNumber] INT NULL,
		[WireInstructions] NVARCHAR(100) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_BankId PRIMARY KEY ([BankID]),
		CONSTRAINT FK_AlibabaSellerBank_SellerId FOREIGN KEY ([SellerId]) REFERENCES [AlibabaSeller] ([SellerId]),
	)
END
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'ContractId' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD ContractId INT NULL
	ALTER TABLE AlibabaBuyer ADD CONSTRAINT FK_AlibabaBuyer_ContractId FOREIGN KEY ([ContractId]) REFERENCES [AlibabaContract] ([ContractId])
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'BussinessName' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD BussinessName NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'street1' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD street1 NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'street2' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD street2 NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'City' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD City NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'State' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD [State] NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'Zip' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD Zip NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'Country' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD Country NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'AuthRepFname' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD AuthRepFname NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'AuthRepLname' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD AuthRepLname NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'Phone' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD Phone NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'Fax' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD Fax NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'Email' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD Email NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrderRequestCountLastYear' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD OrderRequestCountLastYear INT NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'ConfirmShippingDocAndAmount' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD ConfirmShippingDocAndAmount BIT NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'FinancingType' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD FinancingType NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'ConfirmReleaseFunds' AND Object_ID = Object_ID(N'AlibabaBuyer'))
BEGIN
	ALTER TABLE AlibabaBuyer ADD ConfirmReleaseFunds BIT NULL
END