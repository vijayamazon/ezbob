SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataTpdDecisionAlertIndividualsNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataTpdDecisionAlertIndividualsNocs
GO

IF TYPE_ID('CallCreditDataTpdDecisionAlertIndividualsNocsList') IS NOT NULL
	DROP TYPE CallCreditDataTpdDecisionAlertIndividualsNocsList
GO

CREATE TYPE CallCreditDataTpdDecisionAlertIndividualsNocsList AS TABLE (
	[CallCreditDataTpdDecisionAlertIndividualsID] BIGINT NULL,
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

CREATE PROCEDURE SaveCallCreditDataTpdDecisionAlertIndividualsNocs
@Tbl CallCreditDataTpdDecisionAlertIndividualsNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataTpdDecisionAlertIndividualsNocs (
		[CallCreditDataTpdDecisionAlertIndividualsID],
		[NoticeType],
		[Refnum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataTpdDecisionAlertIndividualsID],
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


