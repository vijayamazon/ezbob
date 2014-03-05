IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreMobileCode]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StoreMobileCode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StoreMobileCode] 
	(@Phone VARCHAR(11),
	 @Code CHAR(6))
AS
BEGIN
	UPDATE MobileCodes SET Active = 0 WHERE	Phone = @Phone
	
	INSERT INTO	MobileCodes (Phone, Code, Active, GenerationDate) VALUES (@Phone, @Code, 1, getutcdate())
END
GO
