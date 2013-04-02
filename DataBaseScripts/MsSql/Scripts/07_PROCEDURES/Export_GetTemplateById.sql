IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetTemplateById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_GetTemplateById]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetTemplateById]
(
  @pTemplateId BIGINT
)

AS
BEGIN
       select id, uploaddate, exceptiontype,variablesxml, binarybody, signedDocument
       from export_templateslist
       where Id = @pTemplateId;
END;
GO
