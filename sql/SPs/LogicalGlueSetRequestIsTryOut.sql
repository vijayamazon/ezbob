SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSetRequestIsTryOut') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueSetRequestIsTryOut AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueSetRequestIsTryOut
@RequestUniqueID UNIQUEIDENTIFIER,
@NewIsTryOutStatus BIT,
@RequestID BIGINT OUTPUT
AS
BEGIN
	SET @RequestID = NULL

	SELECT
		@RequestID = RequestID
	FROM
		LogicalGlueRequests
	WHERE
		UniqueID = @RequestUniqueID

	IF @RequestID IS NULL
		SET @RequestID = 0
	ELSE
		UPDATE LogicalGlueRequests SET IsTryOut = @NewIsTryOutStatus WHERE RequestID = @RequestID
END
GO
