IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLastOfferDataForApproval]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLastOfferDataForApproval]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLastOfferDataForApproval] 
	(@CustomerId int)
AS
BEGIN
	DECLARE @OfferValidUntil DATETIME, 
			@OfferStart DATETIME
			
	SELECT 
		@OfferValidUntil = ValidFor, 
		@OfferStart = ApplyForLoan 
	FROM 
		Customer 
	WHERE 
		Id = @CustomerId
	
	IF @OfferStart IS NULL
	BEGIN
		DECLARE @ValidForHours INT
		SELECT @OfferStart = GETUTCDATE()
		SELECT @ValidForHours = CONVERT(INT, Value) FROM ConfigurationVariables WHERE Name='OfferValidForHours'
		SET @OfferValidUntil = DATEADD(hh, @ValidForHours ,GETUTCDATE()) 
	END	
	
	SELECT
		EmailSendingBanned,
		@OfferStart AS OfferStart,
		@OfferValidUntil AS OfferValidUntil
	FROM 
		CashRequests cr
	WHERE 
		Id = 
			(
			SELECT 
				Id 
			FROM
				(
				SELECT 
					ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
					cr.Id 
				FROM 
					CashRequests cr
				WHERE 
					cr.IdCustomer = @CustomerId
				) p
			WHERE 
				p.row=1
			)
END
GO
