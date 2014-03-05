IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Log_ServerAction_Insert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Log_ServerAction_Insert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Log_ServerAction_Insert] 
	(@pCommand nvarchar(255),
    @pRequest ntext,
    @pApplicationId int,
    @pUserHost nvarchar(255),
    @pid bigint output)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	INSERT INTO Log_ServiceAction (Command, Request, ApplicationId, UserHost) 
	VALUES (@pCommand, @pRequest, @pApplicationId, @pUserHost)
    -- Insert statements for procedure here
	SELECT @pid = SCOPE_IDENTITY()
END
GO
