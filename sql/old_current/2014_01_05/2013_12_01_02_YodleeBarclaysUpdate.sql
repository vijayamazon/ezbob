UPDATE dbo.YodleeBanks
SET Name = 'Barclays (non Pin Sentry Users) (UK)'
WHERE ContentServiceId = 4721
GO


UPDATE dbo.YodleeBanks
SET Active = 0
WHERE ContentServiceId = 19302
GO

