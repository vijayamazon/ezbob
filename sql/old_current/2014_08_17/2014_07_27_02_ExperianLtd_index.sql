IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IDX_ExperianLtd_ServiceLogID' AND object_id = OBJECT_ID('ExperianLtd'))
	CREATE INDEX IDX_ExperianLtd_ServiceLogID ON ExperianLtd(ExperianLtdID, ServiceLogID)
GO
