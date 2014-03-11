IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIsConsumerDataUpdated]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetIsConsumerDataUpdated]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetIsConsumerDataUpdated] 
	(@CustomerId INT,
	 @DirectorId INT, 
	 @Today DATE)
AS
BEGIN
	DECLARE @LastUpdateTime DATE
			
	IF @DirectorId = 0
	BEGIN
		SELECT @LastUpdateTime = max(LastUpdateDate) FROM MP_ExperianDataCache WHERE CustomerId = @CustomerId AND (DirectorId IS NULL OR DirectorId = 0)
	END
	ELSE
	BEGIN
		SELECT @LastUpdateTime = max(LastUpdateDate) FROM MP_ExperianDataCache WHERE CustomerId = @CustomerId AND DirectorId = @DirectorId
	END
	
	IF @Today = @LastUpdateTime
		SELECT CAST (1 AS BIT) AS IsUpdated
	ELSE
		SELECT CAST (0 AS BIT) AS IsUpdated
END
GO
