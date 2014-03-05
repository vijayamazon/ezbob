IF OBJECT_ID (N'dbo.GetExpensesPayPalTransactionsByPayer') IS NOT NULL
	DROP FUNCTION dbo.GetExpensesPayPalTransactionsByPayer
GO

CREATE FUNCTION [dbo].[GetExpensesPayPalTransactionsByPayer]
(	@marketplaceId int,
  @payer NVARCHAR (255)
)
RETURNS @Expenses TABLE
(
	Payer NVARCHAR (255),
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
	
	SELECT @min_created=MIN(Created) FROM MP_PayPalTransactionItem WHERE Payer=@payer
	SELECT @datediff =DATEDIFF(mm,@min_created,GETDATE())
		
	DECLARE
		@M1 float, @M3 float, @M6 float, @M12 float, @M15 float, @M18 float, @M24 float, @M24Plus float
		
		IF (@datediff>=1)
		begin
		SELECT TOP 1 @M1 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -1, GETDATE()), GETDATE())
		end
		
		IF (@datediff>1)
		begin			
		SELECT TOP 1 @M3 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -3, GETDATE()), GETDATE())
		END
		
		IF (@datediff>3)
		begin
		SELECT TOP 1 @M6 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -6, GETDATE()), GETDATE())
		END
		
		IF (@datediff>6)
		begin
		SELECT TOP 1 @M12 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -12, GETDATE()), GETDATE())
		END
		
		IF (@datediff>12)
		begin
		SELECT TOP 1 @M15 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -15, GETDATE()), GETDATE())
		END
		
		IF (@datediff>15)
		begin
		SELECT TOP 1 @M18 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -18, GETDATE()), GETDATE())
		END
		
		IF (@datediff>18)
		begin
		SELECT TOP 1 @M24 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -24, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=24)
		begin
		SELECT TOP 1 @M24Plus = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, '1900-01-01 00:00:00.000', GETDATE())
		END
		
		INSERT INTO @Expenses (Payer, M1, M3 , M6 , M12, M15, M18, M24, M24Plus) VALUES (@payer, @M1, @M3, @M6, @M12, @M15, @M18, @M24, @M24Plus)

RETURN

GO

