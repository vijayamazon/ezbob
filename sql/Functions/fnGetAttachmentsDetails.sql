IF OBJECT_ID (N'dbo.fnGetAttachmentsDetails') IS NOT NULL
	DROP FUNCTION dbo.fnGetAttachmentsDetails
GO

CREATE FUNCTION [dbo].[fnGetAttachmentsDetails]
(	@appId int
)
RETURNS TABLE 
AS
RETURN 
(
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

