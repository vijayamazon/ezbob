IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[RegisteredCustomers]'))
DROP VIEW [dbo].[RegisteredCustomers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
