SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
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
