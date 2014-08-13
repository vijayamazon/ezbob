SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveSmsMessage') IS NOT NULL
	DROP PROCEDURE SaveSmsMessage
GO

IF TYPE_ID('SmsMessageList') IS NOT NULL
	DROP TYPE SmsMessageList
GO

CREATE TYPE SmsMessageList AS TABLE (
	UserId INT NULL,
	UnderwriterId INT NOT NULL,
	Sid NVARCHAR(255) NULL,
	DateCreated DATETIME NOT NULL,
	DateUpdated DATETIME NOT NULL,
	DateSent DATETIME NOT NULL,
	AccountSid NVARCHAR(255) NULL,
	[From] NVARCHAR(255) NULL,
	[To] NVARCHAR(255) NULL,
	Body NVARCHAR(255) NULL,
	Status NVARCHAR(255) NULL,
	Direction NVARCHAR(255) NULL,
	Price DECIMAL(18,6) NOT NULL,
	ApiVersion NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveSmsMessage
@Tbl SmsMessageList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO SmsMessage (
		UserId,
		UnderwriterId,
		Price,
		ApiVersion,
		Direction,
		Status,
		Body,
		[To],
		[From],
		AccountSid,
		DateSent,
		DateUpdated,
		DateCreated,
		Sid
		
	) SELECT
		UserId,
		UnderwriterId,
		Price,
		ApiVersion,
		Direction,
		Status,
		Body,
		[To],
		[From],
		AccountSid,
		DateSent,
		DateUpdated,
		DateCreated,
		Sid
	FROM @Tbl
END
GO