SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataCifasPlusCasesFilingReasons') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataCifasPlusCasesFilingReasons
GO

IF TYPE_ID('CallCreditDataCifasPlusCasesFilingReasonsList') IS NOT NULL
	DROP TYPE CallCreditDataCifasPlusCasesFilingReasonsList
GO

CREATE TYPE CallCreditDataCifasPlusCasesFilingReasonsList AS TABLE (
	[CallCreditDataCifasPlusCasesID] BIGINT NULL,
	[FilingReason] NVARCHAR(10) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataCifasPlusCasesFilingReasons
@Tbl CallCreditDataCifasPlusCasesFilingReasonsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataCifasPlusCasesFilingReasons table.', 11, 1)

	INSERT INTO CallCreditDataCifasPlusCasesFilingReasons (
		[CallCreditDataCifasPlusCasesID],
		[FilingReason]
	) SELECT
		[CallCreditDataCifasPlusCasesID],
		[FilingReason]
	FROM @Tbl
END
GO


