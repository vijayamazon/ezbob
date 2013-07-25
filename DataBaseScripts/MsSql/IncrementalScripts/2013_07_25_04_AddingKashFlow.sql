IF NOT EXISTS (SELECT 1 FROM MP_MarketplaceType WHERE Description='KashFlow.com')
	INSERT INTO MP_MarketplaceType VALUES ('KashFlow', 'A755B4F6-D4EC-4D80-96A2-B2849BD800AC', 'KashFlow.com', 1)
GO
