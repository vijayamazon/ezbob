IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_LinkTemplate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_LinkTemplate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_LinkTemplate] 
	(@pTemplateId INT,
    @pNodeId INT,
    @pOutputType INT,
    @pIsLinked INT)
AS
BEGIN
	delete from export_templatenoderel
   where templateid = @pTemplateId and nodeid = @pNodeId;
   
   IF @pIsLinked is not null 
   BEGIN
	insert into export_templatenoderel
         (templateid, nodeid, outputtype)
       values
         (@pTemplateId, @pNodeId, @pOutputType);
   END
END
GO
