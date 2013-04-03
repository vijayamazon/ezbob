IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetAttachmentsDetails]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetAttachmentsDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[fnGetAttachmentsDetails]
(
	@appId int
)
RETURNS 
TABLE 
AS
return(
	select attachments.* 
	from vAppDetails attachments
	where attachments.ParentDetailId in 
	(
		select det.DetailId 
		from vAppDetails det 
		where det.ParentDetailId = 
			(
				select detroot.DetailId from vAppDetails detroot where detroot.Name = 'Root' and detroot.ApplicationId = @appId
			)
		and Name = 'Attachments'
	)
)
GO
