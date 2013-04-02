IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateMPErrorMP]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateMPErrorMP]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateMPErrorMP] 
(@umi int,
 @UpdateError varchar(max),
 @TokenExpired int)

AS
BEGIN

UPDATE [dbo].[MP_CustomerMarketPlace]
   SET  [UpdateError] = @UpdateError,
		TokenExpired = @TokenExpired

 WHERE Id = @umi



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
