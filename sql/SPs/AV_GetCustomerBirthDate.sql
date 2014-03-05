IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetCustomerBirthDate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetCustomerBirthDate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_GetCustomerBirthDate] 
	(@CustomerId INT)
AS
BEGIN
	SELECT DateOfBirth FROM Customer WHERE Id=@CustomerId
END
GO
