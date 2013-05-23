IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'ParentBank' AND id = OBJECT_ID('YodleeBanks'))
BEGIN
	ALTER TABLE YodleeBanks ADD ParentBank NVARCHAR(100) NOT NULL DEFAULT ('abc')
	UPDATE YodleeBanks SET ParentBank = 'HSBC' WHERE ContentServiceId IN (4567,17583,12279)
	UPDATE YodleeBanks SET ParentBank = 'Barclays' WHERE ContentServiceId IN (4721,18539,19302)
	UPDATE YodleeBanks SET ParentBank = 'Lloyds' WHERE ContentServiceId IN (16284,4495)
	UPDATE YodleeBanks SET ParentBank = 'RBS' WHERE ContentServiceId IN (19505,13203)
	UPDATE YodleeBanks SET ParentBank = 'Santander' WHERE ContentServiceId IN (4416,20344)
	UPDATE YodleeBanks SET ParentBank = 'NatWest' WHERE ContentServiceId IN (19008,10249)
	UPDATE YodleeBanks SET ParentBank = 'DAG' WHERE ContentServiceId IN (11195)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Active' AND id = OBJECT_ID('YodleeBanks'))
BEGIN
	ALTER TABLE YodleeBanks ADD Active BIT NOT NULL DEFAULT (1)
END
GO

