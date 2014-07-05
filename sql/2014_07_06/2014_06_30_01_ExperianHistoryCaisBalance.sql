IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CaisBalance' AND id = OBJECT_ID('MP_ExperianHistory'))
	ALTER TABLE MP_ExperianHistory ADD CaisBalance DECIMAL(18,4)
GO