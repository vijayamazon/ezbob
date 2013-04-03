IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetExportFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_GetExportFile]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetExportFile]
(
       @fileId int
)
AS
BEGIN
   SELECT
          Export_Results.FileName
        , Export_Results.BinaryBody
        , Export_Results.ApplicationId
        , Export_Results.NodeName
        , Export_Results.SignedDocumentId
   FROM Export_Results
   WHERE export_results.id = @fileId;

END
GO
