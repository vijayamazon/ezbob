SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_DecisionRejectReasonsSave') IS NOT NULL
	DROP PROCEDURE  NL_DecisionRejectReasonsSave
GO

IF TYPE_ID('NL_DecisionRejectReasonsList') IS NOT NULL
	DROP TYPE NL_DecisionRejectReasonsList
GO

CREATE TYPE NL_DecisionRejectReasonsList AS TABLE (
	[DecisionID] BIGINT NOT NULL,
	[RejectReasonID] INT NOT NULL
)
GO

CREATE PROCEDURE NL_DecisionRejectReasonsSave
@Tbl NL_DecisionRejectReasonsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_DecisionRejectReasons(
		[DecisionID],
		[RejectReasonID] 
	) SELECT
		[DecisionID],
		[RejectReasonID] 
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO
