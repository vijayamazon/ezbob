IF OBJECT_ID('RegisteredCustomers') IS NOT NULL
	DROP VIEW RegisteredCustomers
GO

CREATE VIEW RegisteredCustomers AS
SELECT
	this_.Id,
	this_.Name,
	this_.IsSuccessfullyRegistered,
	CASE WHEN (SELECT COUNT(*) FROM [MP_CustomerMarketPlace] c where c.UpdatingEnd is null and c.CustomerId = this_.Id) > 0 THEN 'not updated' ELSE 'updated' END as MPStatus, 
	this_.GreetingMailSentDate,
	dbo.GetMarketPlaceStatus (1, this_.Id) as EbayStatus, 
	dbo.GetMarketPlaceStatus (2, this_.Id) as AmazonStatus, 
	dbo.GetMarketPlaceStatus (3, this_.Id) as PayPalStatus, 
	dbo.GetMarketPlaceStatus (4, this_.Id) as EkmStatus, 
	this_.WizardStep
FROM
	Customer this_
WHERE
	this_.CreditResult is null
GO

