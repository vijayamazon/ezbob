IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetYodleeAccount]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetYodleeAccount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetYodleeAccount] 
	(@IsCustomerId BIT,
	@Id INT)
AS
BEGIN
	IF @IsCustomerId = 1 
		SELECT Username, Password 
		FROM YodleeAccounts 
		WHERE CustomerId=@Id
	ELSE 
		SELECT Username, Password 
		FROM YodleeAccounts 
		WHERE Id=@Id
END
GO
