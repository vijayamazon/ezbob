IF OBJECT_ID ('dbo.ExperianAccountStatuses') IS NOT NULL
BEGIN
	PRINT 'ExperianAccountStatuses exists'
	DROP TABLE ExperianAccountStatuses
END
ELSE
BEGIN
	CREATE TABLE dbo.ExperianAccountStatuses
	(
		Id VARCHAR(3) NOT NULL,
		Status VARCHAR(10),
		DetailedStatus VARCHAR(100),
		CONSTRAINT PK_ExperianAccountStatuses PRIMARY KEY (Id)
	)

	INSERT INTO ExperianAccountStatuses VALUES ('0', 'OK', '0 days')
	INSERT INTO ExperianAccountStatuses VALUES ('1', '30', '30 days')
	INSERT INTO ExperianAccountStatuses VALUES ('2', '60', '60 days')
	INSERT INTO ExperianAccountStatuses VALUES ('3', '90', '90 days')
	INSERT INTO ExperianAccountStatuses VALUES ('4', '120', '120 days')
	INSERT INTO ExperianAccountStatuses VALUES ('5', '150', '150 days')
	INSERT INTO ExperianAccountStatuses VALUES ('6', '180', '180 days')
	INSERT INTO ExperianAccountStatuses VALUES ('8', 'Def', 'Default')
	INSERT INTO ExperianAccountStatuses VALUES ('9', 'Bad', 'Bad Debt')
	INSERT INTO ExperianAccountStatuses VALUES ('S', 'Slow', 'Slow Payer')
	INSERT INTO ExperianAccountStatuses VALUES ('U', 'U', 'Unclassified')
	INSERT INTO ExperianAccountStatuses VALUES ('D', 'Dorm', 'Dormant')
	INSERT INTO ExperianAccountStatuses VALUES ('?', '?', 'Unknown')

END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

