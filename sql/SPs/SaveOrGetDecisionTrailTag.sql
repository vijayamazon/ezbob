SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveOrGetDecisionTrailTag') IS NULL
	EXECUTE('CREATE PROCEDURE SaveOrGetDecisionTrailTag AS SELECT 1')
GO

ALTER PROCEDURE SaveOrGetDecisionTrailTag
@Tag NVARCHAR(256),
@TagID BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	SET @TagID = NULL

	------------------------------------------------------------------------------

	SET @Tag = ISNULL(LTRIM(RTRIM(ISNULL(@Tag, ''))), '')

	------------------------------------------------------------------------------

	IF @Tag = ''
		SET @Tag = NULL

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF @Tag IS NOT NULL
	BEGIN
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

		BEGIN TRANSACTION

		SELECT
			@TagID = TrailTagID
		FROM
			DecisionTrailTags
		WITH
			(UPDLOCK)
		WHERE
			TrailTag = @Tag

		-------------------------------------------------------------------------

		IF @TagID IS NULL
		BEGIN
			INSERT INTO DecisionTrailTags (TrailTag) VALUES (@Tag)

			SET @TagID = SCOPE_IDENTITY()
		END

		COMMIT TRANSACTION
	END
END
GO
