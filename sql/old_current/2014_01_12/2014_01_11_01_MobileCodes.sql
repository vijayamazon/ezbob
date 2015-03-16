IF OBJECT_ID('MobileCodes') IS NULL
BEGIN
	CREATE TABLE MobileCodes (
		Id INT IDENTITY NOT NULL,
		Phone VARCHAR(13) NOT NULL,
		Code CHAR(6) NOT NULL,
		Active BIT
	)
END
ELSE
BEGIN
	DECLARE @PhoneLength INT
	
	SELECT @PhoneLength = IC.CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS IC WHERE TABLE_NAME = 'MobileCodes' AND COLUMN_NAME = 'Phone'
	IF (@PhoneLength != 13)
	BEGIN
		ALTER TABLE MobileCodes ALTER COLUMN Phone VARCHAR(13)
	END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'GenerationDate' and Object_ID = Object_ID(N'MobileCodes'))    
BEGIN
	ALTER TABLE MobileCodes ADD GenerationDate DATETIME
END 
GO

IF NOT EXISTS (SELECT 1 FROM MobileCodes WHERE Phone = '01111111111')
BEGIN
	INSERT INTO	MobileCodes (Phone, Code, Active, GenerationDate) VALUES ('01111111111', '222222', 1, '2010 Jan 1')
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
	(@Phone VARCHAR(11),
	 @Code CHAR(6))
AS
BEGIN
	UPDATE MobileCodes SET Active = 0 WHERE	Phone = @Phone
	
	INSERT INTO	MobileCodes (Phone, Code, Active, GenerationDate) VALUES (@Phone, @Code, 1, getutcdate())
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCurrentMobileCodeCount]') AND type in (N'P', N'PC'))
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
