IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAttachmentsCount]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetAttachmentsCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Alexey Gorbach
-- Create date: 21.05.2010
-- Description:	Returns attachments count for specific attachments control
-- =============================================
CREATE FUNCTION GetAttachmentsCount 
(
	@appId int,
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
