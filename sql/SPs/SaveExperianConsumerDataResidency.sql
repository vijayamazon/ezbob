SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianConsumerDataResidency') IS NOT NULL
	DROP PROCEDURE SaveExperianConsumerDataResidency
GO

IF TYPE_ID('ExperianConsumerDataResidencyList') IS NOT NULL
	DROP TYPE ExperianConsumerDataResidencyList
GO

CREATE TYPE ExperianConsumerDataResidencyList AS TABLE (
	ExperianConsumerDataId BIGINT NULL,
	ApplicantIdentifier INT NULL,
	LocationIdentifier INT NULL,
	LocationCode NVARCHAR(255) NULL,
	ResidencyDateFrom DATETIME NULL,
	ResidencyDateTo DATETIME NULL,
	TimeAtYears NVARCHAR(255) NULL,
	TimeAtMonths NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianConsumerDataResidency
@Tbl ExperianConsumerDataResidencyList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianConsumerDataResidency (
		ExperianConsumerDataId,
		ApplicantIdentifier,
		LocationIdentifier,
		LocationCode,
		ResidencyDateFrom,
		ResidencyDateTo,
		TimeAtYears,
		TimeAtMonths
	) SELECT
		ExperianConsumerDataId,
		ApplicantIdentifier,
		LocationIdentifier,
		LocationCode,
		ResidencyDateFrom,
		ResidencyDateTo,
		TimeAtYears,
		TimeAtMonths
	FROM @Tbl
END
GO