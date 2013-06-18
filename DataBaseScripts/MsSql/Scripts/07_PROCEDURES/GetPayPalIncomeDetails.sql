IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPayPalIncomeDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPayPalIncomeDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 
*/
CREATE PROCEDURE [dbo].[GetPayPalIncomeDetails]
	@marketPlaceId int
AS
BEGIN
	CREATE TABLE #Incomes
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

DECLARE @name VARCHAR (max)
DECLARE tp_Cursor CURSOR FOR 
select Payer from [GetBiggestIncomePayPalTransactions]( @marketPlaceId)
OPEN tp_Cursor;
FETCH NEXT FROM tp_Cursor INTO @name;
WHILE @@FETCH_STATUS = 0
begin
	insert INTO #Incomes select * from [GetIncomePayPalTransactionsByPayer]( @marketPlaceId, @name) 
FETCH NEXT FROM tp_Cursor INTO @name;
end
CLOSE tp_Cursor
DEALLOCATE tp_Cursor;

select NEWID() as Id, * from #Incomes
DROP TABLE #Incomes
END
GO
