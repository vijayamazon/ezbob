IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCurrentMobileCodeCount]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCurrentMobileCodeCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCurrentMobileCodeCount] 
	(@Phone VARCHAR(11))
AS
BEGIN
	DECLARE 
		@Today DATETIME,
		@SentToday INT,
		@SentToNumber INT

	SELECT @Today = convert(DATE, getutcdate())
	
	SELECT 
		@SentToday = COUNT(1)
	FROM
		MobileCodes
	WHERE
		convert(DATE, GenerationDate) = @Today
		
	SELECT 
		@SentToNumber = COUNT(1)
	FROM
		MobileCodes
	WHERE
		Phone = @Phone
		
	SELECT
		@SentToday AS SentToday,
		@SentToNumber AS SentToNumber
END
GO
