IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetExportResults]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_GetExportResults]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetExportResults] 
	(@pApplicationId int,
       @pNodeId int)
AS
BEGIN
	select export_results.id,
          export_results.filename,
          export_results.binarybody,
          export_results.filetype,
          export_results.creationdate,
          export_results.applicationid,
          export_results.sourcetemplateid,
          export_results.status,
          export_results.nodename,
          export_results.SignedDocumentId
   from export_results
        inner join export_templatenoderel on
          export_templatenoderel.templateid = export_results.sourcetemplateid
          and export_templatenoderel.outputtype = export_results.filetype
   where export_results.applicationid = @pApplicationId
     and export_templatenoderel.nodeid = @pNodeId
     and export_results.statusmode is null
   union all
   select export_results.id,
          export_results.filename,
          export_results.binarybody,
          export_results.filetype,
          export_results.creationdate,
          export_results.applicationid,
          export_results.sourcetemplateid,
          export_results.status,
          export_results.nodename,
          export_results.SignedDocumentId
   from export_results
        inner join entityLink l ON l.EntityType='ExportTemplate'
          and export_results.sourcetemplateid = l.seriaId
        inner join export_templatenoderel on export_templatenoderel.templateid = l.entityId
          and export_templatenoderel.outputtype = export_results.filetype
   where export_results.applicationid = @pApplicationId
     and export_templatenoderel.nodeid = @pNodeId
     and export_results.statusmode is null
   order by export_results.creationdate desc
END
GO
