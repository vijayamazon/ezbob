IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('I_InvestorBankAccountTransaction') AND name='TransactionDate')
BEGIN
	ALTER TABLE I_InvestorBankAccountTransaction DROP COLUMN TimestampCounter 
	ALTER TABLE I_InvestorBankAccountTransaction ADD TransactionDate DATETIME NULL
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
