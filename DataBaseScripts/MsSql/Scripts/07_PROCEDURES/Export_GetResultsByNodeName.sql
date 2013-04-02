IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetResultsByNodeName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_GetResultsByNodeName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetResultsByNodeName]
(
       @pApplicationId int,
       @pNodeName nvarchar(max)
)
AS
BEGIN
   select export_results.id,
          export_results.filename,
          export_results.filetype,
          export_results.creationdate,
          export_results.applicationid,
          export_results.sourcetemplateid,
          export_results.status,
          export_results.nodename,
          export_results.SignedDocumentId
   from export_results
   where export_results.applicationid = @pApplicationId
     and export_results.nodename = @pNodeName;
END
GO
