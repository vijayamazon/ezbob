IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAttachmentByParentDetail]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAttachmentByParentDetail]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAttachmentByParentDetail] 
	(@pDetailId  bigint)
AS
BEGIN
	SELECT apat.[value] FROM Application_Attachment apat
	WHERE apat.DetailId = 
	(
		SELECT ad.DetailId
		FROM Application_Detail ad 
		INNER JOIN Application_DetailName dn ON ad.DetailNameId = dn.DetailNameId
		WHERE
			ad.ParentDetailId = @pDetailId and
			dn.Name = 'FileBin'
	)
    RETURN
END
GO
