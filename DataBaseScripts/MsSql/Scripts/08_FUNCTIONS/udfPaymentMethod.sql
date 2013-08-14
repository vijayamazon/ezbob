IF OBJECT_ID('dbo.udfPaymentMethod') IS NOT NULL
	DROP FUNCTION dbo.udfPaymentMethod
GO

CREATE FUNCTION dbo.udfPaymentMethod(
	@Description NVARCHAR(64)
)
RETURNS NVARCHAR(64)
AS
BEGIN
	IF @Description IS NULL
		RETURN NULL
		
	DECLARE @Prefix NVARCHAR(64) = 'Manual payment method: '

	IF @Description NOT LIKE @Prefix + '%'
		RETURN NULL

	DECLARE @PrefixLen INT = LEN(@Prefix)

	DECLARE @CommaPos INT = CHARINDEX(',', @Description, @PrefixLen + 1)

	IF @CommaPos = 0
		RETURN NULL

	RETURN SUBSTRING(@Description, @PrefixLen + 2, @CommaPos - @PrefixLen - 2)
END
GO
