IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ErrorMessageSave]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ErrorMessageSave]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ErrorMessageSave]
	@pApplicationId         BIGINT,
	@pErrorMessage          NVARCHAR(3000)
AS
BEGIN
	DECLARE @pExist int;
	SELECT @pExist=ae.ApplicationId FROM Application_Error ae WHERE ae.ApplicationId = @pApplicationId;
	if (@pExist is not null)
		UPDATE [Application_Error] SET [ErrorMessage] = @pErrorMessage WHERE [ApplicationId] = @pApplicationId;
	else
        INSERT INTO [Application_Error]
               ([ApplicationId],
				[ErrorMessage])
        VALUES 
               ( @pApplicationId,
			     @pErrorMessage);

-- Сохраняем данные в таблице истории ошибок
        INSERT INTO [Application_Error_History]
               ([ApplicationId],
				[ErrorMessage])
        VALUES 
               ( @pApplicationId,
			     @pErrorMessage);

END;
GO
