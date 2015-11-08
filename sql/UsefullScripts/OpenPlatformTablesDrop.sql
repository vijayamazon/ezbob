
IF NOT object_id('I_InvestorContact') IS NULL
	DROP TABLE I_InvestorContact
GO

IF NOT object_id('I_InvestorBankAccountTransaction') IS NULL
	DROP TABLE I_InvestorBankAccountTransaction
GO

IF NOT object_id('I_InvestorSystemBalance') IS NULL
	DROP TABLE I_InvestorSystemBalance 
GO

IF NOT object_id('I_InvestorOverallStatistics') IS NULL
	DROP TABLE I_InvestorOverallStatistics
GO

IF NOT object_id('I_InvestorFundsAllocation') IS NULL
	DROP TABLE I_InvestorFundsAllocation
GO

IF NOT object_id('I_InvestorBankAccount') IS NULL
	DROP TABLE I_InvestorBankAccount
GO


IF NOT object_id('I_InvestorAccountType') IS NULL
	DROP TABLE I_InvestorAccountType
GO

IF NOT object_id('I_ProductSubType') IS NULL
	DROP TABLE I_ProductSubType
GO

IF NOT object_id('I_Portfolio') IS NULL
	DROP TABLE I_Portfolio
GO

IF NOT object_id('I_ProductTerm') IS NULL
	DROP TABLE I_ProductTerm
GO

IF NOT object_id('I_Index') IS NULL
	DROP TABLE I_Index
GO

IF NOT object_id('I_ProductType') IS NULL
	DROP TABLE I_ProductType
GO

IF NOT object_id('I_Grade') IS NULL
	DROP TABLE I_Grade
GO

IF NOT object_id('I_Product') IS NULL
	DROP TABLE I_Product
GO

IF NOT object_id('I_InvestorConfigurationParam') IS NULL
	DROP TABLE I_InvestorConfigurationParam
GO

IF NOT object_id('I_Investor') IS NULL
	DROP TABLE I_Investor
GO

IF NOT object_id('I_InvestorType') IS NULL
	DROP TABLE I_InvestorType
GO

IF NOT object_id('I_Parameter') IS NULL
	DROP TABLE I_Parameter
GO

IF NOT object_id('I_InterestVariable') IS NULL
	DROP TABLE I_InterestVariable
GO

IF NOT object_id('I_Instrument') IS NULL
	DROP TABLE I_Instrument
GO
