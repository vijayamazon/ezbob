SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeDirectors') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeDirectors
GO

IF TYPE_ID('CreditSafeDirectorsList') IS NOT NULL
	DROP TYPE CreditSafeDirectorsList
GO

CREATE TYPE CreditSafeDirectorsList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	Title NVARCHAR(10) NULL,
	Name NVARCHAR(100) NULL,
	Address1 NVARCHAR(100) NULL,
	Address2 NVARCHAR(100) NULL,
	Address3 NVARCHAR(100) NULL,
	Address4 NVARCHAR(100) NULL,
	Address5 NVARCHAR(100) NULL,
	Address6 NVARCHAR(100) NULL,
	PostCode NVARCHAR(10) NULL,
	BirthDate DATETIME NULL,
	Nationality NVARCHAR(100) NULL,
	Honours NVARCHAR(100) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeDirectors
@Tbl CreditSafeDirectorsList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @CreditSafeDirectorsID BIGINT

	INSERT INTO CreditSafeDirectors (
		CreditSafeBaseDataID,
		Title,
		Name,
		Address1,
		Address2,
		Address3,
		Address4,
		Address5,
		Address6,
		PostCode,
		BirthDate,
		Nationality,
		Honours
	) SELECT
		CreditSafeBaseDataID,
		Title,
		Name,
		Address1,
		Address2,
		Address3,
		Address4,
		Address5,
		Address6,
		PostCode,
		BirthDate,
		Nationality,
		Honours
	FROM @Tbl
	
	SET @CreditSafeDirectorsID = SCOPE_IDENTITY()

	SELECT @CreditSafeDirectorsID AS CreditSafeDirectorsID
END
GO


