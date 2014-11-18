SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveDecisionTrail') IS NOT NULL
	DROP PROCEDURE SaveDecisionTrail
GO

IF TYPE_ID('DecisionTraceList') IS NOT NULL
	DROP TYPE DecisionTraceList
GO

CREATE TYPE DecisionTraceList AS TABLE (
	Position INT,
	DecisionStatusID INT,
	Name NVARCHAR(255),
	Comment NVARCHAR(MAX)
)
GO

CREATE PROCEDURE SaveDecisionTrail
@CustomerID INT,
@DecisionID INT,
@DecisionTime DATETIME,
@UniqueID UNIQUEIDENTIFIER,
@DecisionStatusID INT,
@InputData NVARCHAR(MAX),
@IsPrimary BIT,
@Traces DecisionTraceList READONLY,
@Notes LongStringList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @TrailID BIGINT

	------------------------------------------------------------------------------

	INSERT INTO DecisionTrail (
		CustomerID, DecisionID, DecisionTime, UniqueID,
		DecisionStatusID, InputData, IsPrimary
	) VALUES (
		@CustomerID, @DecisionID, @DecisionTime, @UniqueID,
		@DecisionStatusID, @InputData, @IsPrimary
	)

	------------------------------------------------------------------------------

	SELECT @TrailID = SCOPE_IDENTITY()

	------------------------------------------------------------------------------

	INSERT INTO DecisionTrace (TrailID, Position, Name, DecisionStatusID, Comment)
	SELECT
		@TrailID,
		Position,
		Name,
		DecisionStatusID,
		Comment
	FROM
		@Traces

	------------------------------------------------------------------------------

	INSERT INTO DecisionTrailNotes (TrailID, TrailNote)
	SELECT
		@TrailID,
		Value
	FROM
		@Notes
END
GO
