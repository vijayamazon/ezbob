IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetYodleePayersInfo]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetYodleePayersInfo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetYodleePayersInfo] 
	(@CustomerId INT)
AS
BEGIN
	SELECT DISTINCT 
		Description 
	FROM 
		MP_YodleeOrderItemBankTransaction 
	WHERE 
		OrderItemId IN 
		(
			SELECT 
				Id 
			FROM 
				MP_YodleeOrderItem 
			WHERE 
				OrderId IN 
				(
					SELECT 
						Id 
					FROM 
						MP_YodleeOrder 
					WHERE 
						CustomerMarketPlaceId IN
						(
							SELECT 
								Id
							FROM
								MP_CustomerMarketplace
							WHERE
								CustomerId = @CustomerId
						)
				)
		)
END
GO
