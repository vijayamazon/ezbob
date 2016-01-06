SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_GradeOriginMapSave') IS NOT NULL
	DROP PROCEDURE I_GradeOriginMapSave
GO

IF TYPE_ID('I_GradeOriginMapList') IS NOT NULL
	DROP TYPE I_GradeOriginMapList
GO

CREATE TYPE I_GradeOriginMapList AS TABLE (
	[GradeID] INT NOT NULL,
	[OriginID] INT NOT NULL
)
GO

CREATE PROCEDURE I_GradeOriginMapSave
@Tbl I_GradeOriginMapList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_GradeOriginMap (
		[GradeID],
		[OriginID]
	) SELECT
		[GradeID],
		[OriginID]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


