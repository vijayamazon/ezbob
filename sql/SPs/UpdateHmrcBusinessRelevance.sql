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
	BelongsToCustomer BIT
)
GO

CREATE PROCEDURE UpdateHmrcBusinessRelevance
@RelevanceList BusinessRelevanceList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Business SET
		BelongsToCustomer = p.BelongsToCustomer
	FROM
		Business b
		INNER JOIN @RelevanceList p ON b.Id = p.BusinessID
END
GO
