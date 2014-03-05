IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateMPErrorCustomer]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateMPErrorCustomer]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateMPErrorCustomer] 
	(@UserId int,
 @UpdateError varchar(max),
 @TokenExpired int)
AS
BEGIN
	UPDATE [dbo].[MP_CustomerMarketPlace]
   SET  [UpdateError] = @UpdateError,
		TokenExpired = @TokenExpired

 WHERE CustomerId = @UserId



 SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO
