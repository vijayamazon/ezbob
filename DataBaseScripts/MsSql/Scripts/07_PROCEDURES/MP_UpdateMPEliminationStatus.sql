IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_UpdateMPEliminationStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_UpdateMPEliminationStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_UpdateMPEliminationStatus] 
(@iCustomerMarketPlaceId int, @EliminationPassed int)

AS
BEGIN

UPDATE [dbo].[MP_CustomerMarketPlace]
   SET [EliminationPassed] = @EliminationPassed
 WHERE Id = @iCustomerMarketPlaceId


END
GO
