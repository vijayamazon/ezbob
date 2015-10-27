
IF NOT object_id('InvestorContact') IS NULL
	DROP TABLE InvestorContact
GO

IF NOT object_id('InvestorBankAccountTransaction') IS NULL
	DROP TABLE InvestorBankAccountTransaction
GO

IF NOT object_id('InvestorSystemBalance') IS NULL
	DROP TABLE InvestorSystemBalance 
GO

IF NOT object_id('InvestorOverallStatistics') IS NULL
	DROP TABLE InvestorOverallStatistics
GO

IF NOT object_id('InvestorBankAccount') IS NULL
	DROP TABLE InvestorBankAccount
GO

IF NOT object_id('Investor') IS NULL
	DROP TABLE Investor
GO

IF NOT object_id('InvestorType') IS NULL
	DROP TABLE InvestorType
GO


IF NOT object_id('InvestorAccountType') IS NULL
	DROP TABLE InvestorAccountType
GO
