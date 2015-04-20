SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataCifasPlusCasesDmrs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataCifasPlusCasesDmrs
GO

IF TYPE_ID('CallCreditDataCifasPlusCasesDmrsList') IS NOT NULL
	DROP TYPE CallCreditDataCifasPlusCasesDmrsList
GO

CREATE TYPE CallCreditDataCifasPlusCasesDmrsList AS TABLE (
	[CallCreditDataCifasPlusCasesID] BIGINT NULL,
	[dmr] INT NOT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataCifasPlusCasesDmrs
@Tbl CallCreditDataCifasPlusCasesDmrsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataCifasPlusCasesDmrs table.', 11, 1)

	INSERT INTO CallCreditDataCifasPlusCasesDmrs (
		[CallCreditDataCifasPlusCasesID],
		[dmr]
	) SELECT
		[CallCreditDataCifasPlusCasesID],
		[dmr]
	FROM @Tbl
END
GO


