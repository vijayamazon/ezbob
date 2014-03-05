IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_GetKeyNames]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_GetKeyNames]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetKeyNames]
AS
BEGIN
	SELECT * FROM [DataSource_KeyFields]
END
GO
