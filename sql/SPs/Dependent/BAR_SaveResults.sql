SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BAR_SaveResults') IS NOT NULL
	DROP PROCEDURE BAR_SaveResults
GO

IF TYPE_ID('BAR_ResultList') IS NOT NULL
	DROP TYPE BAR_ResultList
GO

CREATE TYPE BAR_ResultList AS TABLE (
	CustomerID INT NOT NULL,
	FirstCashRequestID BIGINT NOT NULL,
	AutoDecisionID INT NULL,
	ManualDecisionID INT NULL,
	HasEnoughData BIT NOT NULL,
	IsOldCustomer BIT NOT NULL,
	HasSignature BIT NOT NULL,
	AutoApproveTrailUniqueID UNIQUEIDENTIFIER NULL
)
GO

CREATE PROCEDURE BAR_SaveResults
@TrailTag NVARCHAR(256),
@Lst BAR_ResultList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @TrailTagID BIGINT

	EXECUTE SaveOrGetDecisionTrailTag @TrailTag, @TrailTagID OUTPUT

	INSERT INTO BAR_Results (
		TrailTagID,
		CustomerID,
		FirstCashRequestID,
		AutoDecisionID,
		ManualDecisionID,
		HasEnoughData,
		IsOldCustomer,
		HasSignature,
		AutoApproveTrailID
	)
	SELECT
		@TrailTagID,
		l.CustomerID,
		l.FirstCashRequestID,
		l.AutoDecisionID,
		l.ManualDecisionID,
		l.HasEnoughData,
		l.IsOldCustomer,
		l.HasSignature,
		t.TrailID
	FROM
		@Lst l
		LEFT JOIN DecisionTrail t ON l.AutoApproveTrailUniqueID = t.UniqueID
END
GO
