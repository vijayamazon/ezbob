IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidateMobileCode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ValidateMobileCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ValidateMobileCode]
	(@Phone CHAR(11),
	 @Code CHAR(6))
AS
BEGIN
	DECLARE @Success BIT
	
	IF EXISTS (SELECT 1 FROM MobileCodes WHERE Active = 1 AND Phone = @Phone AND Code = @Code )
	BEGIN
		SET @Success = 1
	END
	ELSE
	BEGIN
		SET @Success = 0
	END
	
	SELECT @Success AS Success
END
GO
