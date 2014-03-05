IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetTemplateVars]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_GetTemplateVars]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetTemplateVars] 
	(@pTemplateId int)
AS
BEGIN
	select variablesxml
       from export_templateslist
       where id = @pTemplateId
END
GO
