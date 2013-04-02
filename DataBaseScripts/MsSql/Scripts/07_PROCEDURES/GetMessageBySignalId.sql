IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMessageBySignalId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMessageBySignalId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMessageBySignalId]

		@pApplicationId bigint,
		@pMessage image output
AS
BEGIN
    select @pMessage = Message 
    from Signal WITH (NOLOCK)
    where Label like '%[_]' + cast(@pApplicationId as nvarchar)
          and Applicationid = @pApplicationId;

END
GO
