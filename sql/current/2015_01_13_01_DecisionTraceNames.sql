SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF OBJECT_ID('DecisionTraceNames') IS NULL
BEGIN
	CREATE TABLE DecisionTraceNames (
		TraceNameID BIGINT IDENTITY(0, 1) NOT NULL,
		TraceName NVARCHAR(255) NOT NULL,
		IsEnabled BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DecisionTraceNames PRIMARY KEY (TraceNameID),
		CONSTRAINT UC_DecisionTraceNames UNIQUE (TraceName),
		CONSTRAINT CHK_DecisionTraceNames CHECK (LTRIM(RTRIM(TraceName)) != '')
	)
	--
	INSERT INTO DecisionTraceNames(TraceName, IsEnabled) VALUES ('Unknown (just a stub)', 0)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrace') AND name = 'TraceNameID')
BEGIN
	ALTER TABLE DecisionTrace DROP COLUMN TimestampCounter
	--
	ALTER TABLE DecisionTrace ADD TraceNameID BIGINT NOT NULL CONSTRAINT DF_DecisionTrace_NameID DEFAULT (0)
	--
	ALTER TABLE DecisionTrace ADD TimestampCounter ROWVERSION
END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrace') AND name = 'Name')
BEGIN
	EXECUTE('
		SELECT DISTINCT
			Name
		INTO
			#n
		FROM
			DecisionTrace

		INSERT INTO DecisionTraceNames (TraceName, IsEnabled)
		SELECT
			#n.Name,
			1
		FROM
			#n
			LEFT JOIN DecisionTraceNames n ON #n.Name = n.TraceName
		WHERE
			n.TraceNameID IS NULL

		DROP TABLE #n

		UPDATE t SET
			TraceNameID = n.TraceNameID
		FROM
			DecisionTrace t
			INNER JOIN DecisionTraceNames n ON t.Name = n.TraceName

		ALTER TABLE DecisionTrace DROP CONSTRAINT DF_DecisionTrace_NameID

		ALTER TABLE DecisionTrace DROP COLUMN Name

		ALTER TABLE DecisionTrace ADD CONSTRAINT FK_DecisionTrace_TraceName FOREIGN KEY (TraceNameID) REFERENCES DecisionTraceNames (TraceNameID)
	')
END
GO

IF OBJECT_ID('DecisionTrailTags') IS NULL
BEGIN
	CREATE TABLE DecisionTrailTags (
		TrailTagID BIGINT IDENTITY(0, 1) NOT NULL,
		TrailTag NVARCHAR(256) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DecisionTrailTags PRIMARY KEY (TrailTagID),
		CONSTRAINT UC_DecisionTrailTags UNIQUE (TrailTag),
		CONSTRAINT CHK_DecisionTrailTags CHECK (LTRIM(RTRIM(TrailTag)) != '')
	)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'TrailTagID')
BEGIN
	ALTER TABLE DecisionTrail DROP COLUMN TimestampCounter

	ALTER TABLE DecisionTrail ADD TrailTagID BIGINT NULL

	ALTER TABLE DecisionTrail ADD CONSTRAINT FK_DecisionTrail_TrailTag FOREIGN KEY (TrailTagID) REFERENCES DecisionTrailTags (TrailTagID)

	ALTER TABLE DecisionTrail ADD TimestampCounter ROWVERSION
END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'Tag')
BEGIN
	EXECUTE('
		SELECT DISTINCT
			Tag
		INTO
			#t
		FROM
			DecisionTrail

		INSERT INTO DecisionTrailTags (TrailTag)
		SELECT
			#t.Tag
		FROM
			#t
			LEFT JOIN DecisionTrailTags t ON #t.Tag = t.TrailTag
		WHERE
			t.TrailTagID IS NULL

		DROP TABLE #t

		UPDATE t SET
			TrailTagID = n.TrailTagID
		FROM
			DecisionTrail t
			INNER JOIN DecisionTrailTags n ON t.Tag = n.TrailTag

		ALTER TABLE DecisionTrail DROP COLUMN Tag
	')
END
GO
