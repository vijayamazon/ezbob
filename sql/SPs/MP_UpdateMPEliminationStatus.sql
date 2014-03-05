IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_UpdateMPEliminationStatus]') AND TYPE IN (N'P', N'PC'))
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
