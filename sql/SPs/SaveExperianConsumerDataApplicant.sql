SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianConsumerDataApplicant') IS NOT NULL
	DROP PROCEDURE SaveExperianConsumerDataApplicant
GO

IF TYPE_ID('ExperianConsumerDataApplicantList') IS NOT NULL
	DROP TYPE ExperianConsumerDataApplicantList
GO

CREATE TYPE ExperianConsumerDataApplicantList AS TABLE (
	ExperianConsumerDataId BIGINT NULL,
	ApplicantIdentifier INT NULL,
	Title NVARCHAR(255) NULL,
	Forename NVARCHAR(255) NULL,
	MiddleName NVARCHAR(255) NULL,
	Surname NVARCHAR(255) NULL,
	Suffix NVARCHAR(255) NULL,
	DateOfBirth DATETIME NULL,
	Gender NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianConsumerDataApplicant
@Tbl ExperianConsumerDataApplicantList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianConsumerDataApplicant (
		ExperianConsumerDataId,
		ApplicantIdentifier,
		Title,
		Forename,
		MiddleName,
		Surname,
		Suffix,
		DateOfBirth,
		Gender
	) SELECT
		ExperianConsumerDataId,
		ApplicantIdentifier,
		Title,
		Forename,
		MiddleName,
		Surname,
		Suffix,
		DateOfBirth,
		Gender
	FROM @Tbl
END
GO