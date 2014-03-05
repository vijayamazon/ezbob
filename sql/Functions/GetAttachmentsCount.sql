IF OBJECT_ID (N'dbo.GetAttachmentsCount') IS NOT NULL
	DROP FUNCTION dbo.GetAttachmentsCount
GO

CREATE FUNCTION [dbo].[GetAttachmentsCount]
(	@appId int,
	@name nvarchar(max)
)
RETURNS INT 
AS
BEGIN
	DECLARE @cnt INT;
	
	select @cnt = count(*) from vAppDetails
	where ParentDetailId = 
		(
			select DetailId from vAppDetails
			where ParentDetailId = 
			(
				SELECT DetailId
					from vAppDetails
				  where ApplicationId = @appId and Name = 'Attachments'
			  ) and Name = @name
		);
		RETURN @cnt;
END

GO

