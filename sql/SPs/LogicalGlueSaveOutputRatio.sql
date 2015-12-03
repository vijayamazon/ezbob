SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveOutputRatio') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveOutputRatio
GO

IF TYPE_ID('LogicalGlueOutputRatioList') IS NOT NULL
	DROP TYPE LogicalGlueOutputRatioList
GO

CREATE TYPE LogicalGlueOutputRatioList AS TABLE (
	[OutputClass] NVARCHAR(255) NULL,
	[Score] DECIMAL(18, 6) NOT NULL,
	[ModelOutputID] BIGINT NOT NULL
)
GO

CREATE PROCEDURE LogicalGlueSaveOutputRatio
@Tbl LogicalGlueOutputRatioList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO LogicalGlueModelOutputRatios (
		[ModelOutputID] ,
		[OutputClass],
		[Score]
	) SELECT
		[ModelOutputID] ,
		[OutputClass],
		[Score]
	FROM
		@Tbl
END
GO
