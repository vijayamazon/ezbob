IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorBankAccountTransaction') AND name='TransactionDate')
BEGIN
	ALTER TABLE I_InvestorBankAccountTransaction DROP COLUMN TimestampCounter 
	ALTER TABLE I_InvestorBankAccountTransaction ADD TransactionDate DATETIME NULL
	ALTER TABLE I_InvestorBankAccountTransaction ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorBankAccountTransaction') AND name='BankTransactionRef')
BEGIN
	ALTER TABLE I_InvestorBankAccountTransaction DROP COLUMN TimestampCounter 
	ALTER TABLE I_InvestorBankAccountTransaction ADD BankTransactionRef NVARCHAR(50) NULL 
	ALTER TABLE I_InvestorBankAccountTransaction ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorSystemBalance') AND name='TransactionDate') 
BEGIN
	ALTER TABLE I_InvestorSystemBalance DROP COLUMN TimestampCounter 
	ALTER TABLE I_InvestorSystemBalance ADD TransactionDate DATETIME NULL
	ALTER TABLE I_InvestorSystemBalance ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorSystemBalance') AND name='UserID') 
BEGIN
	ALTER TABLE I_InvestorSystemBalance DROP COLUMN TimestampCounter
	ALTER TABLE I_InvestorSystemBalance ADD UserID INT NULL
	ALTER TABLE I_InvestorSystemBalance ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorBankAccountTransaction') AND name='Comment') 
BEGIN
	ALTER TABLE I_InvestorSystemBalance DROP COLUMN TimestampCounter
	ALTER TABLE I_InvestorSystemBalance ADD Comment NVARCHAR(500)
	ALTER TABLE I_InvestorSystemBalance ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorSystemBalance') AND name='NLOfferID') 
BEGIN
	ALTER TABLE I_InvestorSystemBalance DROP COLUMN TimestampCounter
	ALTER TABLE I_InvestorSystemBalance ADD NLOfferID BIGINT NULL
	ALTER TABLE I_InvestorSystemBalance ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorSystemBalance') AND name='NLLoanID') 
BEGIN
	ALTER TABLE I_InvestorSystemBalance DROP COLUMN TimestampCounter
	ALTER TABLE I_InvestorSystemBalance ADD NLLoanID BIGINT NULL
	ALTER TABLE I_InvestorSystemBalance ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorSystemBalance') AND name='NLPaymentID') 
BEGIN
	ALTER TABLE I_InvestorSystemBalance DROP COLUMN TimestampCounter
	ALTER TABLE I_InvestorSystemBalance ADD NLPaymentID BIGINT NULL
	ALTER TABLE I_InvestorSystemBalance ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_OpenPlatformOffer') AND name='NLOfferID') 
BEGIN
	ALTER TABLE I_OpenPlatformOffer DROP COLUMN TimestampCounter
	ALTER TABLE I_OpenPlatformOffer ADD NLOfferID BIGINT NULL
	ALTER TABLE I_OpenPlatformOffer ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_Portfolio') AND name='NLLoanID') 
BEGIN
	ALTER TABLE I_Portfolio DROP COLUMN TimestampCounter
	ALTER TABLE I_Portfolio ADD NLLoanID BIGINT NULL
	ALTER TABLE I_Portfolio ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_Index') AND name='ProductID') 
BEGIN
	ALTER TABLE I_Index DROP COLUMN TimestampCounter
	ALTER TABLE I_Index ADD ProductID INT NULL
	ALTER TABLE I_Index ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorContact') AND name='IsGettingAlerts') 
BEGIN
	ALTER TABLE I_InvestorContact DROP COLUMN TimestampCounter
	ALTER TABLE I_InvestorContact ADD IsGettingAlerts  BIT  NOT NULL DEFAULT(1)
	ALTER TABLE I_InvestorContact ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorContact') AND name='IsGettingReports') 
BEGIN
	ALTER TABLE I_InvestorContact DROP COLUMN TimestampCounter
	ALTER TABLE I_InvestorContact ADD IsGettingReports BIT   NOT NULL DEFAULT(0)
	ALTER TABLE I_InvestorContact ADD TimestampCounter ROWVERSION
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_Index') AND name='ProductID') 
BEGIN
	ALTER TABLE I_Index DROP COLUMN TimestampCounter
	ALTER TABLE I_Index ADD ProductID INT NULL
	ALTER TABLE I_Index ADD TimestampCounter ROWVERSION
END
GO
