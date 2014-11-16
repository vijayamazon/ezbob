IF OBJECT_ID('DecisionTrace') IS NULL
BEGIN
	CREATE TABLE DecisionTrace (
		TraceID BIGINT IDENTITY(1, 1) NOT NULL,
		TrailID BIGINT NOT NULL,
		Position INT NOT NULL,
		IsPrimary BIT NOT NULL,
		Name NVARCHAR(255) NOT NULL,
		CompletedSuccessfully BIT NOT NULL,
		InitArgs NVARCHAR(MAX) NULL,
		Properties NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DecisionTrace PRIMARY KEY (TraceID),
		CONSTRAINT FK_DecisionTrace_Trail FOREIGN KEY (TrailID) REFERENCES DecisionTrail (TrailID)
	)
END
GO
