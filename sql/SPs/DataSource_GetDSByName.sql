IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_GetDSByName]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_GetDSByName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetDSByName] 
	(@pDataSourceName nvarchar(max))
AS
BEGIN
	select * from datasource_sources
  where (isdeleted is null) AND DisplayName =  @pDataSourceName
END
GO
