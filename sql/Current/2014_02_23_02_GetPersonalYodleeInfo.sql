IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPersonalYodleeInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPersonalYodleeInfo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPersonalYodleeInfo] 
	(@CustomerId INT)
AS
BEGIN	
	DECLARE @EarliestTransactionDate DATETIME
	SELECT
		@EarliestTransactionDate = MIN(PostDate)
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
		
	SELECT 
		@EarliestTransactionDate AS EarliestTransactionDate--,
		--vat
		--sum of loan transactions
END
GO
