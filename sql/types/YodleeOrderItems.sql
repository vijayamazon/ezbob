IF OBJECT_ID('UpdateMpTotalsYodlee') IS NOT NULL
	DROP PROCEDURE UpdateMpTotalsYodlee
GO

IF OBJECT_ID('dbo.udfYodleeFormula') IS NOT NULL
	DROP FUNCTION dbo.udfYodleeFormula
GO

IF TYPE_ID('YodleeOrderItems') IS NOT NULL
	DROP TYPE YodleeOrderItems
GO

SET QUOTED_IDENTIFIER ON
GO
	
CREATE TYPE YodleeOrderItems AS TABLE (
	id INT,
	srcElementId NVARCHAR(300),
	transactionBaseTypeId INT,
	EzbobCategory INT,
	transactionAmount NUMERIC(18, 2),
	theDate DATETIME
)
GO
