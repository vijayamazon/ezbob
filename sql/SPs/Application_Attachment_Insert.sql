IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Attachment_Insert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_Attachment_Insert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Application_Attachment_Insert] 
	(@pDetailId bigint,
		@pValue image,	   
        @pAttachmentId bigint output)
AS
BEGIN
	insert into Application_Attachment
     (Value, DetailId)
      values
     (@pValue, @pDetailId)

     select @pAttachmentId = SCOPE_IDENTITY()
END
GO
