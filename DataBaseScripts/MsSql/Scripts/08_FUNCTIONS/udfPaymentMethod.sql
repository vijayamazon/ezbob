IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[udfPaymentMethod]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[udfPaymentMethod]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
