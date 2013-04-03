IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetNodeTemplateLinks]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_GetNodeTemplateLinks]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetNodeTemplateLinks]
(
  @pTemplateId INT
)
AS
BEGIN
	select * from dbo.Export_TemplateNodeRel
	where TemplateId = @pTemplateId
END;
GO
