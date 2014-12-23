IF OBJECT_ID('UpdateMpTotalsPayPal') IS NOT NULL
	DROP PROCEDURE UpdateMpTotalsPayPal
GO

IF OBJECT_ID('dbo.udfPayPalFormula') IS NOT NULL
	DROP FUNCTION dbo.udfPayPalFormula
GO

IF TYPE_ID('PayPalOrderItems') IS NOT NULL
	DROP TYPE PayPalOrderItems
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TYPE PayPalOrderItems AS TABLE (
	Id INT,
	Created DATETIME,
	NetAmount NUMERIC(18, 2),
	[Type] NVARCHAR(128),
	[Status] NVARCHAR(128)
)
GO
