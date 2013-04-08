IF OBJECT_ID ('dbo.ExperianDataAgentConfigs') IS NOT NULL
BEGIN
	PRINT 'ExperianDataAgentConfigs exists'
	DROP TABLE ExperianDataAgentConfigs
END

CREATE TABLE dbo.ExperianDataAgentConfigs
(
	CfgKey VARCHAR(100) NOT NULL,
	CfgValue VARCHAR(100),
	CONSTRAINT PK_ExperianDataAgentConfigs PRIMARY KEY (CfgKey)
)

INSERT INTO ExperianDataAgentConfigs VALUES ('IntervalSeconds', '6000')
INSERT INTO ExperianDataAgentConfigs VALUES ('FirstIdToHandle', '0')
INSERT INTO ExperianDataAgentConfigs VALUES ('LastIdToHandle', '10000000')
INSERT INTO ExperianDataAgentConfigs VALUES ('MaxBucketSize', '100')

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

