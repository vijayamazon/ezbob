IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrokerLogin]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[BrokerLogin]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BrokerLogin] 
	(@Email NVARCHAR(255),
@Password NVARCHAR(255))
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255) = ''

	IF @ErrMsg = ''
	BEGIN
		IF NOT EXISTS (SELECT * FROM Broker WHERE ContactEmail = @Email AND Password = @Password)
			SET @ErrMsg = 'Invalid contact person email or password.'
	END

	SELECT @ErrMsg AS ErrorMsg
END
GO
