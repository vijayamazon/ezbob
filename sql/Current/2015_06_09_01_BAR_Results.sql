SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BAR_Results') IS NOT NULL
	DROP TABLE BAR_Results
GO

CREATE TABLE BAR_Results (
	ResultID BIGINT IDENTITY(1, 1) NOT NULL,
	TrailTagID BIGINT NOT NULL,
	CustomerID INT NOT NULL,
	FirstCashRequestID BIGINT NOT NULL,
	AutoDecisionID INT NULL,
	ManualDecisionID INT NULL,
	HasEnoughData BIT NOT NULL,
	IsOldCustomer BIT NOT NULL,
	HasSignature BIT NOT NULL,
	AutoApproveTrailID BIGINT NULL,
	TimestampCounter ROWVERSION,
	CONSTRAINT PK_BAR_Results PRIMARY KEY (ResultID),
	CONSTRAINT FK_BAR_Results_TrailTag FOREIGN KEY (TrailTagID) REFERENCES DecisionTrailTags (TrailTagID),
	CONSTRAINT FK_BAR_Results_Customer FOREIGN KEY (CustomerID) REFERENCES Customer (Id),
	CONSTRAINT FK_BAR_Results_FirstCashRequestID FOREIGN KEY (FirstCashRequestID) REFERENCES CashRequests (Id),
	CONSTRAINT FK_BAR_Results_AutoDecision FOREIGN KEY (AutoDecisionID) REFERENCES Decisions (DecisionID),
	CONSTRAINT FK_BAR_Results_AutoApproveTrail FOREIGN KEY (AutoApproveTrailID) REFERENCES DecisionTrail (TrailID)
)
GO
