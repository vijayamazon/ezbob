IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIsCustomerHomeOwnerAccordingToLandRegistry]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetIsCustomerHomeOwnerAccordingToLandRegistry]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetIsCustomerHomeOwnerAccordingToLandRegistry] 
	(@CustomerId INT)
AS
BEGIN
	IF EXISTS (SELECT 1 FROM CustomerAddress WHERE CustomerId = @CustomerId AND IsOwnerAccordingToLandRegistry = 1)
		SELECT CAST(1 AS BIT) AS IsOwner
	ELSE
		SELECT CAST(0 AS BIT) AS IsOwner
END
GO
