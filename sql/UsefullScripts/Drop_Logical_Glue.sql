SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
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

IF OBJECT_ID('LogicalGlueResponses') IS NOT NULL
	DROP TABLE LogicalGlueResponses
GO

IF OBJECT_ID('LogicalGlueBuckets') IS NOT NULL
	DROP TABLE LogicalGlueBuckets
GO

IF OBJECT_ID('LogicalGlueRequestTypes') IS NOT NULL
	DROP TABLE LogicalGlueRequestTypes
GO

IF OBJECT_ID('LogicalGlueRequests') IS NOT NULL
	DROP TABLE LogicalGlueRequests
GO

DELETE FROM ConfigurationVariables WHERE Name IN (
	'LogicalGlueCacheAcceptanceDays',
	'LogicalGlueHostName',
	'LogicalGlueNewCustomerRequestPath',
	'LogicalGlueOldCustomerRequestPath'
)
GO
