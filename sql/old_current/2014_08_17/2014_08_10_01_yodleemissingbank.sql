
IF NOT EXISTS (SELECT * FROM YodleeBanks WHERE ContentServiceId=22292) 
INSERT INTO dbo.YodleeBanks	(Name, ContentServiceId, ParentBank, Active, [Image]) VALUES ('Metro Bank Business Corporate (UK)', 22292, '', 1, 0)
GO

UPDATE YodleeBanks SET Name='Metro Bank Retail (UK)' WHERE ContentServiceId=19540
GO
