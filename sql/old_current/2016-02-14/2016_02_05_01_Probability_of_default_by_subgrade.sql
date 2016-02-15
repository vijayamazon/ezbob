SET QUOTED_IDENTIFIER ON

SELECT
	Name AS SubGradeName,
	CONVERT(DECIMAL(18, 6), NULL) AS ProbOfDefault
INTO
	#pd
FROM
	I_SubGrade
WHERE
	1 = 0
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('I_SubGrade') AND name = 'DefaultRate')
BEGIN
	ALTER TABLE I_SubGrade DROP COLUMN TimestampCounter

	ALTER TABLE I_SubGrade ADD DefaultRate DECIMAL(18, 6) NULL

	ALTER TABLE I_SubGrade ADD TimestampCounter ROWVERSION

	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('A1', 0.00742115027829313)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('A2', 0.0114302461899179)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('A3', 0.0175913396481732)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('A4', 0.0251974426476119)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('B1', 0.0321167883211679)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('B2', 0.0420586607636967)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('C1', 0.0640368852459016)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('D1', 0.0827996340347667)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('D2', 0.0994764397905759)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('E1', 0.107267780800622)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('E2', 0.139335281227173)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('F1', 0.184370477568741)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('G1', 0.212217194570136)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('G2', 0.25)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('G3', 0.295994065281899)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('H1', 0.351877607788595)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('H2', 0.401081081081081)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('H3', 0.486111111111111)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('H4', 0.587837837837838)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('H5', 0.674620390455532)
	INSERT INTO #pd (SubGradeName, ProbOfDefault) VALUES ('H6', 0.922077922077922)
END
GO

IF EXISTS (SELECT * FROM #pd)
BEGIN
	UPDATE I_SubGrade SET
		DefaultRate = ProbOfDefault
	FROM
		I_SubGrade s
		INNER JOIN #pd ON s.Name = #pd.SubGradeName
END
GO

DROP TABLE #pd
GO
