SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE DecisionTrail ALTER COLUMN IsPrimary INT NOT NULL
GO

IF OBJECT_ID('DecisionTrailPrimaryStatus') IS NULL
BEGIN
	CREATE TABLE DecisionTrailPrimaryStatus (
		PrimaryStatusID INT NOT NULL,
		PrimaryStatus NVARCHAR(64) NOT NULL,
		Description NVARCHAR(512) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DecisionTrailPrimaryStatus PRIMARY KEY (PrimaryStatusID),
		CONSTRAINT UC_DecisionTrailPrimaryStatus UNIQUE (PrimaryStatus),
		CONSTRAINT CHK_DecisionTrailPrimaryStatus CHECK (LTRIM(RTRIM(PrimaryStatus)) != '')
	)

	INSERT INTO DecisionTrailPrimaryStatus (PrimaryStatusID, PrimaryStatus, Description) VALUES
		(0, 'Verification', 'Production by current verification flow.'),
		(1, 'Primary', 'Produced by current main flow.'),
		(2, 'Old verification', 'Produced by old verification flow.'),
		(3, 'Old primary', 'Produced by old main flow.')
END
GO

IF OBJECT_ID('FK_DecisionTrail_IsPrimary') IS NULL
	ALTER TABLE DecisionTrail ADD CONSTRAINT FK_DecisionTrail_IsPrimary FOREIGN KEY (IsPrimary) REFERENCES DecisionTrailPrimaryStatus (PrimaryStatusID)
GO
