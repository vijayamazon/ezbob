SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdDL65') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdDL65
GO

IF TYPE_ID('ExperianLtdDL65List') IS NOT NULL
	DROP TYPE ExperianLtdDL65List
GO

CREATE TYPE ExperianLtdDL65List AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	ChargeNumber NVARCHAR(255) NULL,
	FormNumber NVARCHAR(255) NULL,
	CurrencyIndicator NVARCHAR(255) NULL,
	TotalAmountOfDebentureSecured NVARCHAR(255) NULL,
	ChargeType NVARCHAR(255) NULL,
	AmountSecured NVARCHAR(255) NULL,
	PropertyDetails NVARCHAR(4000) NULL,
	ChargeeText NVARCHAR(255) NULL,
	RestrictingProvisions NVARCHAR(255) NULL,
	RegulatingProvisions NVARCHAR(255) NULL,
	AlterationsToTheOrder NVARCHAR(255) NULL,
	PropertyReleasedFromTheCharge NVARCHAR(255) NULL,
	AmountChargeIncreased NVARCHAR(255) NULL,
	CreationDate DATETIME NULL,
	DateFullySatisfied DATETIME NULL,
	FullySatisfiedIndicator NVARCHAR(255) NULL,
	NumberOfPartialSatisfactionDates INT NULL,
	NumberOfPartialSatisfactionDataItems INT NULL
)
GO

CREATE PROCEDURE SaveExperianLtdDL65
@Tbl ExperianLtdDL65List READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ExperianLtdDL65ID BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c != 1
		RAISERROR('Invalid argument: no/too much data to insert into ExperianLtdDL65 table.', 11, 1)

	INSERT INTO ExperianLtdDL65 (
		ExperianLtdID,
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
	) SELECT
		ExperianLtdID,
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
	FROM @Tbl

	SET @ExperianLtdDL65ID = SCOPE_IDENTITY()

	SELECT @ExperianLtdDL65ID AS ExperianLtdID
END
GO
