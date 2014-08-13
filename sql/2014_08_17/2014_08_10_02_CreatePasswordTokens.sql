SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UC_CreatePasswordTokens') IS NOT NULL
	ALTER TABLE CreatePasswordTokens DROP CONSTRAINT UC_CreatePasswordTokens
GO
