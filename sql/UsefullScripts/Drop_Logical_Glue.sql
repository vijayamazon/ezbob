SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueLoadInference') IS NOT NULL
	DROP PROCEDURE LogicalGlueLoadInference
GO

IF OBJECT_ID('LogicalGlueResponseMissingColumns') IS NOT NULL
	DROP TABLE LogicalGlueResponseMissingColumns
GO

IF OBJECT_ID('LogicalGlueResponseEncodingFailures') IS NOT NULL
	DROP TABLE LogicalGlueResponseEncodingFailures
GO

IF OBJECT_ID('LogicalGlueResponseWarnings') IS NOT NULL
	DROP TABLE LogicalGlueResponseWarnings
GO

IF OBJECT_ID('LogicalGlueResponseOutputRatios') IS NOT NULL
	DROP TABLE LogicalGlueResponseOutputRatios
GO

IF OBJECT_ID('LogicalGlueResponseMapOutputRatios') IS NOT NULL -- obsolete table
	DROP TABLE LogicalGlueResponseMapOutputRatios
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
