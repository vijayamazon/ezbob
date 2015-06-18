SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeDirectors_Directorships') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeDirectors_Directorships
GO

IF TYPE_ID('CreditSafeDirectors_DirectorshipsList') IS NOT NULL
	DROP TYPE CreditSafeDirectors_DirectorshipsList
GO

CREATE TYPE CreditSafeDirectors_DirectorshipsList AS TABLE (
	CreditSafeDirectorsID BIGINT NULL,
	CompanyNumber NVARCHAR(100) NULL,
	CompanyName NVARCHAR(500) NULL,
	CompanyStatus NVARCHAR(100) NULL,
	[Function] NVARCHAR(100) NULL,
	AppointedDate DATETIME NULL
)
GO

CREATE PROCEDURE SaveCreditSafeDirectors_Directorships
@Tbl CreditSafeDirectors_DirectorshipsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeDirectors_Directorships (
		CreditSafeDirectorsID,
		CompanyNumber,
		CompanyName,
		CompanyStatus,
		[Function],
		AppointedDate
	) SELECT
		CreditSafeDirectorsID,
		CompanyNumber,
		CompanyName,
		CompanyStatus,
		[Function],
		AppointedDate
	FROM @Tbl
END
GO


