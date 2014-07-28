SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianConsumerDataLocation') IS NOT NULL
	DROP PROCEDURE SaveExperianConsumerDataLocation
GO

IF TYPE_ID('ExperianConsumerDataLocationList') IS NOT NULL
	DROP TYPE ExperianConsumerDataLocationList
GO

CREATE TYPE ExperianConsumerDataLocationList AS TABLE (
	ExperianConsumerDataId BIGINT NULL,
	LocationIdentifier INT NULL,
	Flat NVARCHAR(255) NULL,
	HouseName NVARCHAR(255) NULL,
	HouseNumber NVARCHAR(255) NULL,
	Street NVARCHAR(255) NULL,
	Street2 NVARCHAR(255) NULL,
	District NVARCHAR(255) NULL,
	District2 NVARCHAR(255) NULL,
	PostTown NVARCHAR(255) NULL,
	County NVARCHAR(255) NULL,
	Postcode NVARCHAR(255) NULL,
	POBox NVARCHAR(255) NULL,
	Country NVARCHAR(255) NULL,
	SharedLetterbox NVARCHAR(255) NULL,
	FormattedLocation NVARCHAR(255) NULL,
	LocationCode NVARCHAR(255) NULL,
	TimeAtYears NVARCHAR(255) NULL,
	TimeAtMonths NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianConsumerDataLocation
@Tbl ExperianConsumerDataLocationList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianConsumerDataLocation (
		ExperianConsumerDataId,
		LocationIdentifier,
		Flat,
		HouseName,
		HouseNumber,
		Street,
		Street2,
		District,
		District2,
		PostTown,
		County,
		Postcode,
		POBox,
		Country,
		SharedLetterbox,
		FormattedLocation,
		LocationCode,
		TimeAtYears,
		TimeAtMonths
	) SELECT
		ExperianConsumerDataId,
		LocationIdentifier,
		Flat,
		HouseName,
		HouseNumber,
		Street,
		Street2,
		District,
		District2,
		PostTown,
		County,
		Postcode,
		POBox,
		Country,
		SharedLetterbox,
		FormattedLocation,
		LocationCode,
		TimeAtYears,
		TimeAtMonths
	FROM @Tbl
END
GO