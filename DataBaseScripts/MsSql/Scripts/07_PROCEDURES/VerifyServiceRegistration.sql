IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VerifyServiceRegistration]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[VerifyServiceRegistration]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE VerifyServiceRegistration 
(
	  @pKey nvarchar(255)
	, @pResult nvarchar(255) out
)
AS
BEGIN
	DECLARE @regKey nvarchar(255);
	
	select top 1 @regKey = [key] from ServiceRegistration
	if(@regKey IS NULL)
		INSERT INTO ServiceRegistration
			([key])
		VALUES
			(@pKey);
	else
		if(@regKey <> @pKey)
		begin
			SELECT @pResult = @regKey;
			RETURN;
		end;
	SELECT @pResult = NULL;
END
GO
