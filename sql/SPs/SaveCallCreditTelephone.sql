SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditTelephone') IS NOT NULL
	DROP PROCEDURE SaveCallCreditTelephone
GO

IF TYPE_ID('CallCreditTelephoneList') IS NOT NULL
	DROP TYPE CallCreditTelephoneList
GO

CREATE TYPE CallCreditTelephoneList AS TABLE (
	[CallCreditID] BIGINT NULL,
	[TelephoneType] NVARCHAR(10) NULL,
	[STD] NVARCHAR(5) NULL,
	[PhoneNumber] NVARCHAR(11) NULL,
	[Extension] NVARCHAR(5) NULL
)
GO

CREATE PROCEDURE SaveCallCreditTelephone
@Tbl CallCreditTelephoneList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditTelephone (
		[CallCreditID],
		[TelephoneType],
		[STD],
		[PhoneNumber],
		[Extension]
	) SELECT
		[CallCreditID],
		[TelephoneType],
		[STD],
		[PhoneNumber],
		[Extension]
	FROM @Tbl
END
GO


