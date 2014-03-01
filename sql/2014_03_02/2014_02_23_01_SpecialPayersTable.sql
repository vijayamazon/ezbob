IF OBJECT_ID('SpecialPayers') IS NULL
BEGIN
	CREATE TABLE SpecialPayers 
	(
		Id INT IDENTITY NOT NULL,
		Name NVARCHAR(255) NOT NULL
	)
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetSpecialPayers') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetSpecialPayers
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetSpecialPayers 
AS
BEGIN
	SELECT Name FROM SpecialPayers
END
GO
