IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_AddExportResult]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_AddExportResult]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_AddExportResult] 
	(@pFileName nvarchar(max),
    @pFileType int,
    @pSourceTemplateId int,
    @pBinaryBody varbinary(max),
    @pApplicationId int,
    @pStatus int,
    @pNodeName nvarchar(max))
AS
BEGIN
	update export_results
     set statusmode = 1
   where id in (
	   select id from export_results
	   where applicationid = @pApplicationId and sourcetemplateid = @pSourceTemplateId
	   and filetype = @pFileType
		union
		   select e.Id from export_results e
				inner join entityLink l ON l.EntityType='ExportTemplate'
				  and e.sourcetemplateid = l.EntityId
			where e.applicationid = @pApplicationId 
			and l.SeriaId = @pSourceTemplateId
			and e.filetype = @pFileType
   )
   
   insert into export_results
     (filename, binarybody, filetype,
      sourcetemplateid, applicationid, status, NodeName)
   values
     (@pFileName, @pBinaryBody, @pFileType,
       @pSourceTemplateId, @pApplicationId, @pStatus, @pNodeName);

   SELECT @@IDENTITY
END
GO
