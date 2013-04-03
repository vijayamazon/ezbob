IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetTemplateLinks]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_GetTemplateLinks]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetTemplateLinks]
(
  @pNodeId INT,
  @pShowHistory INT
)

AS
BEGIN
       select id, displayname, terminationdate,
       (select outputtype from export_templatenoderel
       where templateid=id and nodeid = @pNodeId) as "outputtype"
       from export_templateslist
       where isdeleted is null
       and 
       ((@pShowHistory is null and terminationdate is null)
       or (@pShowHistory is not null))
       order BY displayname ASC,case when terminationdate is null then 0 else 1 end, terminationdate desc;

END;
GO
