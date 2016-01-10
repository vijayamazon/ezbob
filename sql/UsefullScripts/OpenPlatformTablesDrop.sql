SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogcialGlueRequestItems') IS NOT NULL
	DROP TABLE LogcialGlueRequestItems
GO

IF OBJECT_ID('LogicalGlueRequestFeatureCategories') IS NOT NULL
	DROP TABLE LogicalGlueRequestFeatureCategories
GO

IF OBJECT_ID('LogicalGlueRequestFeatureTypes') IS NOT NULL
	DROP TABLE LogicalGlueRequestFeatureTypes
GO

IF OBJECT_ID('LogicalGlueRequestItemValueTypes') IS NOT NULL
	DROP TABLE LogicalGlueRequestItemValueTypes
GO

IF OBJECT_ID('LogicalGlueRequests') IS NOT NULL
	DROP TABLE LogicalGlueRequests
GO

IF OBJECT_ID('LogicalGlueRequestSchema') IS NOT NULL
	DROP TABLE LogicalGlueRequestSchema
GO

IF OBJECT_ID('LogicalGlueRequestTypes') IS NOT NULL
	DROP TABLE LogicalGlueRequestTypes
GO

IF OBJECT_ID('LogicalGlueSaveEtlData') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveEtlData
GO

IF OBJECT_ID('LogicalGlueSaveWarning') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveWarning
GO

IF OBJECT_ID('LogicalGlueSaveOutputRatio') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveOutputRatio
GO

IF OBJECT_ID('LogicalGlueSaveMissingColumn') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveMissingColumn
GO

IF OBJECT_ID('LogicalGlueSaveModelOutput') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveModelOutput
GO

IF OBJECT_ID('LogicalGlueSaveEncodingFailure') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveEncodingFailure
GO

IF OBJECT_ID('LogicalGlueSaveResponse') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveResponse
GO

IF OBJECT_ID('LogicalGlueSaveRawResponse') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveRawResponse
GO

IF OBJECT_ID('LogicalGlueSaveInferenceRequest') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveInferenceRequest
GO

IF OBJECT_ID('LogicalGlueLoadInputData') IS NOT NULL
	DROP PROCEDURE LogicalGlueLoadInputData
GO

IF OBJECT_ID('LogicalGlueLoadInference') IS NOT NULL
	DROP PROCEDURE LogicalGlueLoadInference
GO

IF OBJECT_ID('LogicalGlueModelMissingColumns') IS NOT NULL
	DROP TABLE LogicalGlueModelMissingColumns
GO

IF OBJECT_ID('LogicalGlueResponseMissingColumns') IS NOT NULL -- obsolete table
	DROP TABLE LogicalGlueResponseMissingColumns
GO

IF OBJECT_ID('LogicalGlueModelEncodingFailures') IS NOT NULL
	DROP TABLE LogicalGlueModelEncodingFailures
GO

IF OBJECT_ID('LogicalGlueResponseEncodingFailures') IS NOT NULL -- obsolete table
	DROP TABLE LogicalGlueResponseEncodingFailures
GO

IF OBJECT_ID('LogicalGlueModelWarnings') IS NOT NULL
	DROP TABLE LogicalGlueModelWarnings
GO

IF OBJECT_ID('LogicalGlueResponseWarnings') IS NOT NULL -- obsolete table
	DROP TABLE LogicalGlueResponseWarnings
GO

IF OBJECT_ID('LogicalGlueModelOutputRatios') IS NOT NULL
	DROP TABLE LogicalGlueModelOutputRatios
GO

IF OBJECT_ID('LogicalGlueResponseOutputRatios') IS NOT NULL -- obsolete table
	DROP TABLE LogicalGlueResponseOutputRatios
GO

IF OBJECT_ID('LogicalGlueResponseMapOutputRatios') IS NOT NULL -- obsolete table
	DROP TABLE LogicalGlueResponseMapOutputRatios
GO

IF OBJECT_ID('LogicalGlueModelOutputs') IS NOT NULL
	DROP TABLE LogicalGlueModelOutputs
GO

IF OBJECT_ID('LogicalGlueEtlData') IS NOT NULL
	DROP TABLE LogicalGlueEtlData
GO

IF OBJECT_ID('LogicalGlueResponses') IS NOT NULL
	DROP TABLE LogicalGlueResponses
GO

IF OBJECT_ID('LogicalGlueEtlCodes') IS NOT NULL
	DROP TABLE LogicalGlueEtlCodes
GO

IF OBJECT_ID('LogicalGlueTimeoutSources') IS NOT NULL
	DROP TABLE LogicalGlueTimeoutSources
GO

IF OBJECT_ID('LogicalGlueBuckets') IS NOT NULL
	DROP TABLE LogicalGlueBuckets
GO

IF OBJECT_ID('LogicalGlueModels') IS NOT NULL
	DROP TABLE LogicalGlueModels
GO

IF OBJECT_ID('LogicalGlueRequestTypes') IS NOT NULL -- obsolete table
	DROP TABLE LogicalGlueRequestTypes
GO

IF OBJECT_ID('LogicalGlueRequests') IS NOT NULL
	DROP TABLE LogicalGlueRequests
GO

DELETE FROM ConfigurationVariables WHERE Name IN (
	'LogicalGlueCacheAcceptanceDays',
	'LogicalGlueHostName',
	'LogicalGlueNewCustomerRequestPath',
	'LogicalGlueOldCustomerRequestPath',
	'LogicalGlueUserName',
	'LogicalGluePassword',
	'LogicalGlueAuthorizationScheme'
)
GO

DELETE FROM MP_ServiceLog WHERE ServiceType IN (
	'LogicalGlue'
)
GO


IF EXISTS (SELECT * FROM syscolumns WHERE name='ProductSubTypeID' AND id=object_id('CashRequests'))
BEGIN
	ALTER TABLE CashRequests DROP CONSTRAINT FK_CashRequests_I_ProductSubType
	ALTER TABLE CashRequests DROP COLUMN ProductSubTypeID
END
GO

IF NOT object_id('I_GradeOriginMap') IS NULL
	DROP TABLE I_GradeOriginMap
GO

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

IF NOT object_id('I_GradeRange') IS NULL
	DROP TABLE I_GradeRange
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


IF NOT object_id('I_Product') IS NULL
	DROP TABLE I_Product
GO

IF NOT object_id('I_InvestorConfigurationParam') IS NULL
	DROP TABLE I_InvestorConfigurationParam
GO

IF NOT object_id('I_FundingType') IS NULL
	DROP TABLE I_FundingType
GO


IF NOT object_id('I_SubGrade') IS NULL
	DROP TABLE I_SubGrade
GO

IF NOT object_id('I_Grade') IS NULL
	DROP TABLE I_Grade
GO
	
IF NOT object_id('I_UWInvestorConfigurationParam') IS NULL
	DROP TABLE I_UWInvestorConfigurationParam
GO

IF NOT object_id('I_OpenPlatformOffer') IS NULL
	DROP TABLE I_OpenPlatformOffer
GO

IF NOT object_id('I_InterestVariable') IS NULL
	DROP TABLE I_InterestVariable
GO

IF NOT object_id('I_Instrument') IS NULL
	DROP TABLE I_Instrument
GO

IF NOT object_id('I_InvestorRule') IS NULL
	DROP TABLE I_InvestorRule
GO

IF NOT object_id('I_Operator') IS NULL
	DROP TABLE I_Operator
GO

IF NOT object_id('I_InvestorParams') IS NULL
	DROP TABLE I_InvestorParams
GO

IF NOT object_id('I_Parameter') IS NULL
	DROP TABLE I_Parameter
GO

IF NOT object_id('I_Investor') IS NULL
	DROP TABLE I_Investor
GO

IF NOT object_id('I_InvestorType') IS NULL
	DROP TABLE I_InvestorType
GO



