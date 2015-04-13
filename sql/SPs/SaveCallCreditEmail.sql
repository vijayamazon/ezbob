SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditEmail') IS NOT NULL
	DROP PROCEDURE SaveCallCreditEmail
GO

IF TYPE_ID('CallCreditEmailList') IS NOT NULL
	DROP TYPE CallCreditEmailList
GO

CREATE TYPE CallCreditEmailList AS TABLE (
	[CallCreditID] BIGINT NULL,
	[EmailType] NVARCHAR(10) NULL,
	[EmailAddress] NVARCHAR(100) NULL
)
GO

CREATE PROCEDURE SaveCallCreditEmail
@Tbl CallCreditEmailList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditEmail (
		[CallCreditID],
		[EmailType],
		[EmailAddress]
	) SELECT
		[CallCreditID],
		[EmailType],
		[EmailAddress]
	FROM @Tbl
END
GO


