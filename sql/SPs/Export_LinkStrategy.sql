IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_LinkStrategy]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_LinkStrategy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_LinkStrategy] 
	(@pTemplateName nvarchar(max),
    @pStrategyId int)
AS
BEGIN
	DECLARE @l_templateId int;

    select @l_templateId=id from  export_templateslist
      where upper(filename) = upper(@pTemplateName) and isdeleted is null;

   if NOT EXISTS(select templateid from  export_templatestratrel
    where templateid = @l_templateId and strategyid = @pStrategyId)
	BEGIN
       insert into export_templatestratrel
         (templateid, strategyid)
       values
         (@l_templateId, @pStrategyId);
	END
END
GO
