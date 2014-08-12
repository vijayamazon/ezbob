IF OBJECT_ID('SaveSourceRefHistory') IS NOT NULL
	DROP PROCEDURE SaveSourceRefHistory
GO

IF TYPE_ID('SourceRefHistoryList') IS NOT NULL
	DROP TYPE SourceRefHistoryList
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TYPE SourceRefHistoryList AS TABLE (
	SourceRef NVARCHAR(1000) NOT NULL,
	VisitTime DATETIME NULL
)
GO

CREATE PROCEDURE SaveSourceRefHistory
@UserID INT,
@Lst SourceRefHistoryList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO SourceRefHistory (UserID, SourceRef, VisitTime)
	SELECT
		@UserID,
		SourceRef,
		VisitTime
	FROM
		@Lst
END
GO
