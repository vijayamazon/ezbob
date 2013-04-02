IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_GetDataSourcesList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_GetDataSourcesList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetDataSourcesList]
(
  @pDataSourceType nvarchar(255)
)
AS
BEGIN
  select
   ds.ID,
   ds.NAME,
   ds.Description,
   ds.TYPE,
   ds.CREATIONDATE,
   ds.TERMINATIONDATE,
   ds.DISPLAYNAME,
   usr.username
  from datasource_sources ds 
  left outer join security_user usr on ds.userid = usr.userid
  WHERE ((ds.IsDeleted = 0) OR (ds.IsDeleted IS NULL))
   AND ds.Type=@pDataSourceType;
	

END
GO
