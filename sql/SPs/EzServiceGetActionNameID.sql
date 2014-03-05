IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EzServiceGetActionNameID]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[EzServiceGetActionNameID]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EzServiceGetActionNameID] 
	(@ActionName NVARCHAR(255),
@ActionNameID INT OUTPUT)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		@ActionNameID = ActionNameID
	FROM
		EzServiceActionName
	WHERE
		ActionName = @ActionName

	IF @ActionNameID IS NULL
	BEGIN
		INSERT INTO EzServiceActionName (ActionName) VALUES (@ActionName)

		SET @ActionNameID = SCOPE_IDENTITY()
	END
END
GO
