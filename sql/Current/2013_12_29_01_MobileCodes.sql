IF OBJECT_ID('MobileCodes') IS NULL
BEGIN
	CREATE TABLE MobileCodes (
		Id INT IDENTITY NOT NULL,
		MobilePhone CHAR(11) NOT NULL,
		Code CHAR(6) NOT NULL,
		Active BIT
	)
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreMobileCode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StoreMobileCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StoreMobileCode]
	(@MobilePhone CHAR(11),
	 @Code CHAR(6))
AS
BEGIN
	UPDATE MobileCodes SET Active = 0 WHERE	MobilePhone = @MobilePhone
	
	INSERT INTO	MobileCodes (MobilePhone, Code, Active) VALUES (@MobilePhone, @Code, 1)
END
GO
