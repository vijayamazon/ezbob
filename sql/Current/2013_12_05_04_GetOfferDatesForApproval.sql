IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetOfferDatesForApproval]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetOfferDatesForApproval]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOfferDatesForApproval] 
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
					cr.IdCustomer =@CustomerId
				) p
			WHERE 
				p.row=@RowNum
			)
END
GO
