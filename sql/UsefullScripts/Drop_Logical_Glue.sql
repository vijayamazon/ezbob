SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueResponseEncodingFailures') IS NOT NULL
	DROP TABLE LogicalGlueResponseEncodingFailures
GO

IF OBJECT_ID('LogicalGlueResponseWarnings') IS NOT NULL
	DROP TABLE LogicalGlueResponseWarnings
GO

IF OBJECT_ID('LogicalGlueResponseMapOutputRatios') IS NOT NULL
	DROP TABLE LogicalGlueResponseMapOutputRatios
GO

IF OBJECT_ID('LogicalGlueResponses') IS NOT NULL
	DROP TABLE LogicalGlueResponses
GO

IF OBJECT_ID('LogcialGlueRequestItems') IS NOT NULL
	DROP TABLE LogcialGlueRequestItems
GO

IF OBJECT_ID('LogicalGlueRequests') IS NOT NULL
	DROP TABLE LogicalGlueRequests
GO

IF OBJECT_ID('LogicalGlueRequestTypes') IS NOT NULL
	DROP TABLE LogicalGlueRequestTypes
GO

IF OBJECT_ID('LogicalGlueRequestItemValueTypes') IS NOT NULL
	DROP TABLE LogicalGlueRequestItemValueTypes
GO

IF OBJECT_ID('LogicalGlueRequestFeatureCategories') IS NOT NULL
	DROP TABLE LogicalGlueRequestFeatureCategories
GO

IF OBJECT_ID('LogicalGlueRequestSchema') IS NOT NULL
	DROP TABLE LogicalGlueRequestSchema
GO

IF OBJECT_ID('LogicalGlueRequestFeatureTypes') IS NOT NULL
	DROP TABLE LogicalGlueRequestFeatureTypes
GO
