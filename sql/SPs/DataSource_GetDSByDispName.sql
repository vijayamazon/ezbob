IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_GetDSByDispName]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_GetDSByDispName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetDSByDispName] 
	(@pDisplayName nvarchar(max))
AS
BEGIN
	SELECT *
  FROM DataSource_Sources
  WHERE DisplayName = @pDisplayName
    AND (DataSource_Sources.IsDeleted IS NULL OR DataSource_Sources.IsDeleted = 0)
    AND DataSource_Sources.TerminationDate IS NULL
END
GO
