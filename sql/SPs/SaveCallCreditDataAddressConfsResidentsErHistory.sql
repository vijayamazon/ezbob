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

	DECLARE @CallCreditDataAddressConfsResidentsErHistoryId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAddressConfsResidentsErHistory table.', 11, 1)

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

	SET @CallCreditDataAddressConfsResidentsErHistoryId = SCOPE_IDENTITY()

	SELECT @CallCreditDataAddressConfsResidentsErHistoryId AS CallCreditDataAddressConfsResidentsErHistoryId
END
GO


