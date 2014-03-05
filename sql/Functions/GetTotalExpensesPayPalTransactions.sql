IF OBJECT_ID (N'dbo.GetTotalExpensesPayPalTransactions') IS NOT NULL
	DROP FUNCTION dbo.GetTotalExpensesPayPalTransactions
GO

CREATE FUNCTION [dbo].[GetTotalExpensesPayPalTransactions]
(	@marketplaceId int
)
RETURNS @Expenses TABLE
(
	Payer nvarchar(255),
	M1 float, 
	M3 float,
	M6 float,
	M12 float,
	M15 float,
	M18 float,
	M24 float,
	M24Plus float
)
AS BEGIN 
	DECLARE @min_created DATETIME;
	DECLARE @datediff int;
	
	SELECT @min_created=MIN(pi.Created) FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0  
			
	SELECT @datediff =DATEDIFF(mm,@min_created,GETDATE())
	--SELECT @datediff
		
	DECLARE
		@M1 float, @M3 float, @M6 float, @M12 float, @M15 float, @M18 float, @M24 float, @M24Plus float
		
		IF (@datediff>=1)
		begin
		SELECT TOP 1 @M1 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -1, GETDATE()), GETDATE())
		end
		
		IF (@datediff>1)
		begin			
		SELECT TOP 1 @M3 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -3, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=3)
		begin
		SELECT TOP 1 @M6 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -6, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=6)
		begin
		SELECT TOP 1 @M12 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -12, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=12)
		begin
		SELECT TOP 1 @M15 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -15, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=15)
		begin
		SELECT TOP 1 @M18 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -18, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=18)
		begin
		SELECT TOP 1 @M24 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -24, GETDATE()), GETDATE())
		END
		/*
		IF (@datediff>=24)
		begin
		SELECT TOP 1 @M24Plus = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, '1900-01-01 00:00:00.000', GETDATE())
		END
		*/
		SELECT TOP 1 @M24Plus = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, '1900-01-01 00:00:00.000', GETDATE())				
		INSERT INTO @Expenses (Payer, M1, M3 , M6 , M12, M15, M18, M24, M24Plus) VALUES ('Expenses', @M1, @M3 , @M6 , @M12, @M15, @M18, @M24, @M24Plus)

RETURN
end

GO

