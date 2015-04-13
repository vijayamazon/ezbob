SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAddressConfsResidentsErHistory') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAddressConfsResidentsErHistory
GO

IF TYPE_ID('CallCreditDataAddressConfsResidentsErHistoryList') IS NOT NULL
	DROP TYPE CallCreditDataAddressConfsResidentsErHistoryList
GO

CREATE TYPE CallCreditDataAddressConfsResidentsErHistoryList AS TABLE (
	[CallCreditDataAddressConfsResidentsID] BIGINT NULL,
	[StartDate] DATETIME NULL,
	[EndDate] DATETIME NULL,
	[Optout] BIT NULL,
	[RollingRoll] BIT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAddressConfsResidentsErHistory
@Tbl CallCreditDataAddressConfsResidentsErHistoryList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataAddressConfsResidentsErHistory (
		[CallCreditDataAddressConfsResidentsID],
		[StartDate],
		[EndDate],
		[Optout],
		[RollingRoll]
	) SELECT
		[CallCreditDataAddressConfsResidentsID],
		[StartDate],
		[EndDate],
		[Optout],
		[RollingRoll]
	FROM @Tbl
END
GO


