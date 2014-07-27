SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadFullExperianLtd') IS NULL
	EXECUTE('CREATE PROCEDURE LoadFullExperianLtd AS SELECT 1')
GO

ALTER PROCEDURE LoadFullExperianLtd
@ServiceLogID BIGINT,
@InsertDate DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ExperianLtdID BIGINT

	-- Main table

	EXECUTE LoadExperianLtd @ServiceLogID, @ExperianLtdID OUTPUT

	-- Dependent tables (level 1)

	EXECUTE LoadExperianLtdCaisMonthly @ExperianLtdID
	EXECUTE LoadExperianLtdCreditSummary @ExperianLtdID
	EXECUTE LoadExperianLtdErrors @ExperianLtdID
	EXECUTE LoadExperianLtdDL48 @ExperianLtdID
	EXECUTE LoadExperianLtdDL52 @ExperianLtdID
	EXECUTE LoadExperianLtdDL68 @ExperianLtdID
	EXECUTE LoadExperianLtdDL72 @ExperianLtdID
	EXECUTE LoadExperianLtdDL97 @ExperianLtdID
	EXECUTE LoadExperianLtdDL99 @ExperianLtdID
	EXECUTE LoadExperianLtdDLA2 @ExperianLtdID
	EXECUTE LoadExperianLtdDLB5 @ExperianLtdID
	EXECUTE LoadExperianLtdPrevCompanyNames @ExperianLtdID
	EXECUTE LoadExperianLtdShareholders @ExperianLtdID
	EXECUTE LoadExperianLtdDL65 @ExperianLtdID

	-- Dependent tables (level 2)

	EXECUTE LoadExperianLtdLenderDetails @ExperianLtdID

	-- Metadata

	IF @InsertDate IS NULL
	BEGIN
		SELECT
			'Metadata' AS DatumType,
			InsertDate
		FROM
			MP_ServiceLog
		WHERE
			Id = @ServiceLogID
	END
	ELSE BEGIN
		SELECT
			'Metadata' AS DatumType,
			@InsertDate AS InsertDate
	END
END
GO
