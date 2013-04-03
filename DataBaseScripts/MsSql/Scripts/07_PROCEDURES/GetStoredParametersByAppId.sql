IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStoredParametersByAppId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStoredParametersByAppId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [GetStoredParametersByAppId]
	@pApplicationID  BIGINT
AS
BEGIN
   SET NOCOUNT ON;

   SELECT 
       Application_Detail.DetailId
      ,Application_Detail.DetailNameId
      ,Application_Detail.ParentDetailId
      ,Application_Detail.ValueStr
      ,Application_DetailName.Name
   FROM
      Application_Detail WITH (NOLOCK) INNER JOIN
      Application_DetailName WITH (NOLOCK) ON
          Application_Detail.DetailNameId = Application_DetailName.DetailNameId
   WHERE
      Application_Detail.ApplicationId = @pApplicationID
END
GO
