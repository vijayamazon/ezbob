SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveDecisionTrail') IS NOT NULL
	DROP PROCEDURE SaveDecisionTrail
GO

IF TYPE_ID('DecisionTrailStepTimeList') IS NOT NULL
	DROP TYPE DecisionTrailStepTimeList
GO

IF TYPE_ID('DecisionTraceList') IS NOT NULL
	DROP TYPE DecisionTraceList
GO

IF TYPE_ID('LongStringList') IS NOT NULL
	DROP TYPE LongStringList
GO

CREATE TYPE LongStringList AS TABLE (
	Value NVARCHAR(4000) NULL
)
GO

CREATE TYPE DecisionTraceList AS TABLE (
	Position INT,
	DecisionStatusID INT,
	Name NVARCHAR(255),
	HasLockedDecision BIT,
	Comment NVARCHAR(MAX)
)
GO

CREATE TYPE DecisionTrailStepTimeList AS TABLE (
	Position INT,
	StepTimeNameID BIGINT,
	StepLength FLOAT
)
GO

CREATE PROCEDURE SaveDecisionTrail
@CustomerID INT,
@Amount NUMERIC(18, 2),
@DecisionID INT,
@DecisionTime DATETIME,
@UniqueID UNIQUEIDENTIFIER,
@DecisionStatusID INT,
@InputData NVARCHAR(MAX),
@IsPrimary BIT,
@CashRequestID BIGINT,
@Tag NVARCHAR(256),
@Traces DecisionTraceList READONLY,
@TimerSteps DecisionTrailStepTimeList READONLY,
@Notes LongStringList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @TrailID BIGINT

	DECLARE @TagID BIGINT = NULL

	SET @Tag = ISNULL(LTRIM(RTRIM(ISNULL(@Tag, ''))), '')

	IF @Tag = ''
		SET @Tag = NULL

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF @Tag IS NOT NULL
	BEGIN
		SELECT
			@TagID = TrailTagID
		FROM
			DecisionTrailTags
		WHERE
			TrailTag = @Tag

		-------------------------------------------------------------------------

		IF @TagID IS NULL
		BEGIN
			INSERT INTO DecisionTrailTags (TrailTag) VALUES (@Tag)

			SET @TagID = SCOPE_IDENTITY()
		END
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO DecisionTrail (
		CustomerID, DecisionID, DecisionTime, UniqueID,
		DecisionStatusID, InputData, IsPrimary, CashRequestID, TrailTagID, Amount
	) VALUES (
		@CustomerID, @DecisionID, @DecisionTime, @UniqueID,
		@DecisionStatusID, @InputData, @IsPrimary, @CashRequestID, @TagID, @Amount
	)

	------------------------------------------------------------------------------

	SELECT @TrailID = SCOPE_IDENTITY()

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT DISTINCT
		Name
	INTO
		#n
	FROM
		@Traces

	------------------------------------------------------------------------------

	INSERT INTO DecisionTraceNames (TraceName, IsEnabled)
	SELECT
		#n.Name,
		1
	FROM
		#n
		LEFT JOIN DecisionTraceNames n ON #n.Name = n.TraceName
	WHERE
		n.TraceNameID IS NULL

	------------------------------------------------------------------------------

	DROP TABLE #n

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO DecisionTrace (TrailID, Position, TraceNameID, DecisionStatusID, HasLockedDecision, Comment)
	SELECT
		@TrailID,
		t.Position,
		n.TraceNameID,
		t.DecisionStatusID,
		t.HasLockedDecision,
		t.Comment
	FROM
		@Traces t
		INNER JOIN DecisionTraceNames n ON t.Name = n.TraceName

	------------------------------------------------------------------------------

	INSERT INTO DecisionTrailNotes (TrailID, TrailNote)
	SELECT
		@TrailID,
		Value
	FROM
		@Notes

	------------------------------------------------------------------------------

	INSERT INTO DecisionTrailStepTimes (TrailID, Position, StepTimeNameID, StepLength)
	SELECT
		@TrailID,
		Position,
		StepTimeNameID,
		StepLength
	FROM
		@TimerSteps
END
GO
