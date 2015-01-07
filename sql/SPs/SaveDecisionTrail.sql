SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveDecisionTrail') IS NOT NULL
	DROP PROCEDURE SaveDecisionTrail
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
@Notes LongStringList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @TrailID BIGINT

	------------------------------------------------------------------------------

	INSERT INTO DecisionTrail (
		CustomerID, DecisionID, DecisionTime, UniqueID,
		DecisionStatusID, InputData, IsPrimary, CashRequestID, Tag, Amount
	) VALUES (
		@CustomerID, @DecisionID, @DecisionTime, @UniqueID,
		@DecisionStatusID, @InputData, @IsPrimary, @CashRequestID, @Tag, @Amount
	)

	------------------------------------------------------------------------------

	SELECT @TrailID = SCOPE_IDENTITY()

	------------------------------------------------------------------------------

	INSERT INTO DecisionTrace (TrailID, Position, Name, DecisionStatusID, HasLockedDecision, Comment)
	SELECT
		@TrailID,
		Position,
		Name,
		DecisionStatusID,
		HasLockedDecision,
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
