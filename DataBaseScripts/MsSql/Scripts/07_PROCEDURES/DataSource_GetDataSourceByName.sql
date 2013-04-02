IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_GetDataSourceByName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_GetDataSourceByName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetDataSourceByName]
(
  @pDataSourceName nvarchar(max)
)
AS
BEGIN

  SELECT *
  FROM DataSource_Sources
  WHERE Name = @pDataSourceName
    AND (DataSource_Sources.IsDeleted IS NULL OR DataSource_Sources.IsDeleted = 0);

END
GO
