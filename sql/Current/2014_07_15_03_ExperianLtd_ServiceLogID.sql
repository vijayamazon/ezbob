IF OBJECT_ID('UC_ExperianLtd_ServiceLog') IS NULL
	ALTER TABLE ExperianLtd ADD CONSTRAINT UC_ExperianLtd_ServiceLog UNIQUE (ServiceLogID)
GO
