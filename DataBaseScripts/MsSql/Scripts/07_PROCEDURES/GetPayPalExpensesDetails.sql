IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPayPalExpensesDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPayPalExpensesDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 
*/

CREATE PROCEDURE [dbo].[GetPayPalExpensesDetails]
	@marketPlaceId int
AS
BEGIN
	CREATE TABLE #Incomes
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

DECLARE @name VARCHAR (max)
DECLARE tp_Cursor CURSOR FOR 
select Payer from [GetBiggestExpensesPayPalTransactions]( @marketPlaceId)
OPEN tp_Cursor;
FETCH NEXT FROM tp_Cursor INTO @name;
WHILE @@FETCH_STATUS = 0
begin
	insert INTO #Incomes select * from [GetExpensesPayPalTransactionsByPayer]( @marketPlaceId, @name) 
FETCH NEXT FROM tp_Cursor INTO @name;
end
CLOSE tp_Cursor
DEALLOCATE tp_Cursor;

select * from #Incomes
DROP TABLE #Incomes
END
GO
