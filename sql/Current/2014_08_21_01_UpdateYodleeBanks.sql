UPDATE dbo.YodleeBanks
SET Active = 0
WHERE ContentServiceId = 19345
GO

UPDATE dbo.YodleeBanks
SET   Name = 'Santander(International Saver) (UK)'
	, ParentBank = 'Santander'
	, [Image] = 1
WHERE ContentServiceId = 19507
GO

UPDATE dbo.YodleeBanks
SET Active = 0
WHERE ContentServiceId = 16265
GO

UPDATE dbo.YodleeBanks
SET Active = 0
WHERE ContentServiceId = 18282
GO