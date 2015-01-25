SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UpdateHmrcBusinessRelevance') IS NOT NULL
	DROP PROCEDURE UpdateHmrcBusinessRelevance
GO

IF TYPE_ID('BusinessRelevanceList') IS NOT NULL
	DROP TYPE BusinessRelevanceList
GO

CREATE TYPE BusinessRelevanceList AS TABLE (
	BusinessID INT,
	BelongsToCustomer BIT,
	Name NVARCHAR(1024),
	OtherName NVARCHAR(1024),
	FullName NVARCHAR(1024)
)
GO

CREATE PROCEDURE UpdateHmrcBusinessRelevance
@RelevanceList BusinessRelevanceList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Business SET
		BelongsToCustomer = CASE p.BelongsToCustomer
			WHEN 1 THEN 1
			ELSE CASE
				WHEN DIFFERENCE(p.Name, p.OtherName) >= 3 THEN 1
				ELSE CASE
					WHEN DIFFERENCE(p.Name, p.FullName) >= 3 THEN 1
					ELSE 0
				END
			END
		END
	FROM
		Business b
		INNER JOIN @RelevanceList p ON b.Id = p.BusinessID
END
GO
