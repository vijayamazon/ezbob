SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdDL65') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdDL65 AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdDL65
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdDL65' AS DatumType,
		ExperianLtdDL65ID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
		ChargeNumber,
		FormNumber,
		CurrencyIndicator,
		TotalAmountOfDebentureSecured,
		ChargeType,
		AmountSecured,
		PropertyDetails,
		ChargeeText,
		RestrictingProvisions,
		RegulatingProvisions,
		AlterationsToTheOrder,
		PropertyReleasedFromTheCharge,
		AmountChargeIncreased,
		CreationDate,
		DateFullySatisfied,
		FullySatisfiedIndicator,
		NumberOfPartialSatisfactionDates,
		NumberOfPartialSatisfactionDataItems
	FROM
		ExperianLtdDL65
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO
