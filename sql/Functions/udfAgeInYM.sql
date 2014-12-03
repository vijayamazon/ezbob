IF OBJECT_ID('dbo.udfAgeInYM') IS NOT NULL
	DROP FUNCTION dbo.udfAgeInYM
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfAgeInYM(@SomeDate DATETIME)
RETURNS NVARCHAR(64)
AS
BEGIN
	IF @SomeDate IS NULL
		RETURN ''

	DECLARE @Now DATETIME = GETUTCDATE()

	DECLARE @nMonths INT = ABS(DATEDIFF(month, @SomeDate, @Now))

	DECLARE @nYears INT = @nMonths / 12

	DECLARE @nTail INT = @nMonths - @nYears * 12

	DECLARE @Res NVARCHAR(64)

	IF @nYears = 1
		SET @Res = '1 year'
	ELSE IF @nYears > 1
		SET @Res = CONVERT(NVARCHAR, @nYears) + ' years'

	IF @nTail = 1
		SET @Res = @Res + ' 1 month'
	ELSE IF @nTail > 1
		SET @Res = @Res + ' ' + CONVERT(NVARCHAR, @nTail) + ' months'

	RETURN LTRIM(@Res)
END
GO
