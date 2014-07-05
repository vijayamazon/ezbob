IF object_id('RejectReason') IS NULL
BEGIN
	CREATE TABLE RejectReason
	(
		 Id INT NOT NULL IDENTITY(1,1)
		,Reason NVARCHAR(100)
		,CONSTRAINT PK_RejectReason PRIMARY KEY (Id)
	)

END
GO 

IF NOT EXISTS (SELECT * FROM RejectReason)
BEGIN
	INSERT INTO RejectReason (Reason) VALUES ('low personal score')
	INSERT INTO RejectReason (Reason) VALUES ('low business score')
	INSERT INTO RejectReason (Reason) VALUES ('# personal default(s)')
	INSERT INTO RejectReason (Reason) VALUES ('# company default(s)')
	INSERT INTO RejectReason (Reason) VALUES ('CCJ in personal history')
	INSERT INTO RejectReason (Reason) VALUES ('CCJ in business history')
	INSERT INTO RejectReason (Reason) VALUES ('# of delays of # days')
	INSERT INTO RejectReason (Reason) VALUES ('does not respond')
	INSERT INTO RejectReason (Reason) VALUES ('no AML')
	INSERT INTO RejectReason (Reason) VALUES ('fully utilized')
	INSERT INTO RejectReason (Reason) VALUES ('low sales')
	INSERT INTO RejectReason (Reason) VALUES ('no proof of business existing')
	INSERT INTO RejectReason (Reason) VALUES ('low seniority')
	INSERT INTO RejectReason (Reason) VALUES ('negative tangible equity trend')
	INSERT INTO RejectReason (Reason) VALUES ('re-rejection')
	INSERT INTO RejectReason (Reason) VALUES ('less than 50% of previous loan(s) repaid')
	INSERT INTO RejectReason (Reason) VALUES ('2 loans outstanding')
	INSERT INTO RejectReason (Reason) VALUES ('fraud')
	INSERT INTO RejectReason (Reason) VALUES ('thin file')
END
GO

IF object_id('DecisionHistoryRejectReason') IS NULL
BEGIN
	CREATE TABLE DecisionHistoryRejectReason
	(
		 Id INT NOT NULL IDENTITY(1,1)
		,RejectReasonId INT
		,DecisionHistoryId INT
		,CONSTRAINT PK_DecisionHistoryRejectReason PRIMARY KEY (Id)
		,CONSTRAINT FK_DecisionHistoryRejectReason_RejectReason FOREIGN KEY (RejectReasonId) REFERENCES RejectReason(Id)
		,CONSTRAINT FK_DecisionHistoryRejectReason_DecisionHistory FOREIGN KEY (DecisionHistoryId) REFERENCES DecisionHistory(Id)
	)

END
GO 