IF OBJECT_ID('UpdateExperianDirectorDetails') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateExperianDirectorDetails AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateExperianDirectorDetails
@DirectorID INT,
@Email NVARCHAR(512),
@MobilePhone NVARCHAR(50),
@Line1 NVARCHAR(512),
@Line2 NVARCHAR(512),
@Line3 NVARCHAR(512),
@Town NVARCHAR(512),
@County NVARCHAR(512),
@Postcode NVARCHAR(512)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE ExperianDirectors SET
		Email = @Email,
		MobilePhone = @MobilePhone,
		Line1 = @Line1,
		Line2 = @Line2,
		Line3 = @Line3,
		Town = @Town,
		County = @County,
		Postcode = @Postcode
	WHERE
		ExperianDirectorID = @DirectorID
END
GO
