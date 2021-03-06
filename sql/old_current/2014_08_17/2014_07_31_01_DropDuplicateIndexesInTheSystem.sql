IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Askville_MarketPlaceId_GISSC' AND object_id = OBJECT_ID('Askville'))
	DROP INDEX IX_Askville_MarketPlaceId_GISSC on Askville;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_GreetingMailSentDate_NCSARW' AND object_id = OBJECT_ID('Customer'))
	DROP INDEX IX_Customer_GreetingMailSentDate_NCSARW on Customer;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IG_I' AND object_id = OBJECT_ID('Customer'))
	DROP INDEX IX_Customer_IG_I on Customer;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CustomerLoyaltyProgram_CustomerID' AND object_id = OBJECT_ID('CustomerLoyaltyProgram'))
	DROP INDEX IX_CustomerLoyaltyProgram_CustomerID on CustomerLoyaltyProgram;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_Date' AND object_id = OBJECT_ID('LoanSchedule'))
	DROP INDEX IX_LoanSchedule_Date on LoanSchedule;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanScheduleTransaction_LoanID_ISTPFISS' AND object_id = OBJECT_ID('LoanScheduleTransaction'))
	DROP INDEX IX_LoanScheduleTransaction_LoanID_ISTPFISS on LoanScheduleTransaction;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_AMO2_MarketPlaceId_PD2' AND object_id = OBJECT_ID('MP_AmazonOrderItem'))
	DROP INDEX IX_AMO2_MarketPlaceId_PD2 on MP_AmazonOrderItem;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AA_UCV' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	DROP INDEX IX_MP_AnalyisisFunctionValues_AA_UCV on MP_AnalyisisFunctionValues;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_Include' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	DROP INDEX IX_MP_AnalyisisFunctionValues_Include on MP_AnalyisisFunctionValues;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AFTPI' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	DROP INDEX IX_MP_AnalyisisFunctionValues_AFTPI on MP_AnalyisisFunctionValues;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace_MarketPlaceId' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
	DROP INDEX IX_MP_CustomerMarketPlace_MarketPlaceId on MP_CustomerMarketPlace;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayFeedbackCreatedDateIncludeMUI' AND object_id = OBJECT_ID('MP_EbayFeedback'))
	DROP INDEX IX_MP_EbayFeedbackCreatedDateIncludeMUI on MP_EbayFeedback;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_PayPalTransactionItem2_Type' AND object_id = OBJECT_ID('MP_PayPalTransactionItem2'))
	DROP INDEX IX_MP_PayPalTransactionItem2_Type on MP_PayPalTransactionItem2;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='_dta_index_MP_CustomerMarketPlaceUpdatingHi_7_498100815__K1_2_3_4_5' AND object_id = OBJECT_ID('MP_CustomerMarketPlaceUpdatingHistory'))
	DROP INDEX _dta_index_MP_CustomerMarketPlaceUpdatingHi_7_498100815__K1_2_3_4_5 on MP_CustomerMarketPlaceUpdatingHistory;
GO



