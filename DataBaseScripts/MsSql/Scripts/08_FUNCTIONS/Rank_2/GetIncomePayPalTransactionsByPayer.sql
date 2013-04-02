IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIncomePayPalTransactionsByPayer]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetIncomePayPalTransactionsByPayer]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 
*/
CREATE FUNCTION [dbo].[GetIncomePayPalTransactionsByPayer]
(	
  @marketplaceId int,
  @payer nvarchar(255)
)
RETURNS @Incomes TABLE
(
	Payer NVARCHAR(255),
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
		@M1 FLOAT, @M3 FLOAT, @M6 FLOAT, @M12 FLOAT, @M15 FLOAT, @M18 FLOAT, @M24 FLOAT, @M24Plus float
		
		IF (@datediff>=1)
		begin
		SELECT TOP 1 @M1 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -1, GETDATE()), GETDATE())
		end
		
		IF (@datediff>1)
		begin			
		SELECT TOP 1 @M3 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -3, GETDATE()), GETDATE())
		END
		
		IF (@datediff>3)
		begin
		SELECT TOP 1 @M6 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -6, GETDATE()), GETDATE())
		END
		
		IF (@datediff>6)
		begin
		SELECT TOP 1 @M12 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -12, GETDATE()), GETDATE())
		END
		
		IF (@datediff>12)
		begin
		SELECT TOP 1 @M15 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -15, GETDATE()), GETDATE())
		END
		
		IF (@datediff>15)
		begin
		SELECT TOP 1 @M18 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -18, GETDATE()), GETDATE())
		END
		
		IF (@datediff>18)
		begin
		SELECT TOP 1 @M24 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -24, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=24)
		begin
		SELECT TOP 1 @M24Plus = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, '1900-01-01 00:00:00.000', GETDATE())
		END
		
		INSERT INTO @Incomes (Payer, M1, M3 , M6 , M12, M15, M18, M24, M24Plus) VALUES (@payer, @M1, @M3 , @M6 , @M12, @M15, @M18, @M24, @M24Plus)

RETURN
end
GO
