IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSpecialPayers]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSpecialPayers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSpecialPayers]
AS
BEGIN
	SELECT Name FROM SpecialPayers
END
GO
