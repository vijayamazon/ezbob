IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAppAttachmentsState]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateAppAttachmentsState]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateAppAttachmentsState]
   @pApplicationId  bigint
AS
BEGIN

UPDATE Application_Detail_1
SET [ValueStr] = 
  CASE
    WHEN Application_Detail_1.ValueStr = N'New' THEN N'Saved'
    WHEN Application_Detail_1.ValueStr = N'Deleting' THEN N'Deleted'
  END
FROM Application_DetailName INNER JOIN
     Application_Detail AS Application_Detail_1 ON Application_DetailName.DetailNameId = Application_Detail_1.DetailNameId INNER JOIN
     Application_Attachment INNER JOIN
     Application_Detail ON Application_Attachment.DetailId = Application_Detail.DetailId ON 
     Application_Detail_1.ParentDetailId = Application_Detail.ParentDetailId
WHERE Application_DetailName.Name = N'State'
  AND  (Application_Detail_1.ValueStr = N'New'
    OR  Application_Detail_1.ValueStr = N'Deleting')
  AND Application_Detail_1.ApplicationId = @pApplicationId
  AND Application_Detail.ApplicationId = @pApplicationId

END
GO
