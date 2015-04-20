SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataTpdReviewAlertIndividualsNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataTpdReviewAlertIndividualsNocs
GO

IF TYPE_ID('CallCreditDataTpdReviewAlertIndividualsNocsList') IS NOT NULL
	DROP TYPE CallCreditDataTpdReviewAlertIndividualsNocsList
GO

CREATE TYPE CallCreditDataTpdReviewAlertIndividualsNocsList AS TABLE (
	[CallCreditDataTpdReviewAlertIndividualsID] BIGINT NULL,
	[NoticeType] NVARCHAR(10) NULL,
	[Refnum] NVARCHAR(30) NULL,
	[DateRaised] DATETIME NULL,
	[Text] NVARCHAR(4000) NULL,
	[NameDetails] NVARCHAR(164) NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataTpdReviewAlertIndividualsNocs
@Tbl CallCreditDataTpdReviewAlertIndividualsNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataTpdReviewAlertIndividualsNocs table.', 11, 1)

	INSERT INTO CallCreditDataTpdReviewAlertIndividualsNocs (
		[CallCreditDataTpdReviewAlertIndividualsID],
		[NoticeType],
		[Refnum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataTpdReviewAlertIndividualsID],
		[NoticeType],
		[Refnum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	FROM @Tbl
END
GO


