SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

CREATE TABLE #shares (
	GradeID INT NOT NULL,
	TurnoverShare DECIMAL(18, 6) NULL,
	ValueAddedShare DECIMAL(18, 6) NULL,
	FreeCashFlowShare DECIMAL(18, 6) NULL
)

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('I_GradeRange') AND name = 'TurnoverShare')
BEGIN
	ALTER TABLE I_GradeRange DROP COLUMN TimestampCounter

	ALTER TABLE I_GradeRange ADD TurnoverShare DECIMAL(18, 6) NULL
	ALTER TABLE I_GradeRange ADD ValueAddedShare DECIMAL(18, 6) NULL
	ALTER TABLE I_GradeRange ADD FreeCashFlowShare DECIMAL(18, 6) NULL

	INSERT INTO #shares(GradeID, TurnoverShare, ValueAddedShare, FreeCashFlowShare)
		SELECT GradeID, 0.15, 0.58, 0.30 FROM I_Grade WHERE Name = 'A'
	INSERT INTO #shares(GradeID, TurnoverShare, ValueAddedShare, FreeCashFlowShare)
		SELECT GradeID, 0.12, 0.53, 0.28 FROM I_Grade WHERE Name = 'B'
	INSERT INTO #shares(GradeID, TurnoverShare, ValueAddedShare, FreeCashFlowShare)
		SELECT GradeID, 0.10, 0.48, 0.25 FROM I_Grade WHERE Name = 'C'
	INSERT INTO #shares(GradeID, TurnoverShare, ValueAddedShare, FreeCashFlowShare)
		SELECT GradeID, 0.08, 0.38, 0.20 FROM I_Grade WHERE Name = 'D'
	INSERT INTO #shares(GradeID, TurnoverShare, ValueAddedShare, FreeCashFlowShare)
		SELECT GradeID, 0.06, 0.29, 0.15 FROM I_Grade WHERE Name = 'E'
	INSERT INTO #shares(GradeID, TurnoverShare, ValueAddedShare, FreeCashFlowShare)
		SELECT GradeID, 0.06, 0.29, 0.15 FROM I_Grade WHERE Name = 'F'
	INSERT INTO #shares(GradeID, TurnoverShare, ValueAddedShare, FreeCashFlowShare)
		SELECT GradeID, 0.06, 0.25, 0.12 FROM I_Grade WHERE Name = 'G'

	ALTER TABLE I_GradeRange ADD TimestampCounter ROWVERSION
END
GO

IF EXISTS (SELECT * FROM #shares)
BEGIN
	UPDATE I_GradeRange SET
		TurnoverShare = s.TurnoverShare,
		ValueAddedShare = s.ValueAddedShare,
		FreeCashFlowShare = s.FreeCashFlowShare
	FROM
		I_GradeRange r
		INNER JOIN #shares s ON r.GradeID = s.GradeID
END

DROP TABLE #shares
GO
