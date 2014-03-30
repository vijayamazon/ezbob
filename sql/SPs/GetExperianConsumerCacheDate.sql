IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianConsumerCacheDate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianConsumerCacheDate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianConsumerCacheDate]
	(@CustomerId INT,
	 @DirectorId INT)
AS
BEGIN	
	IF @DirectorId = 0
	BEGIN
		SELECT
			MIN(LastUpdateDate) AS LastUpdateDate
		FROM
			MP_ExperianDataCache
		WHERE
			CustomerId = @CustomerId AND 
			Name IS NOT NULL AND
			(
				DirectorId IS NULL OR 
				DirectorId = 0
			)
	END
	ELSE
	BEGIN
		SELECT
			MIN(LastUpdateDate) AS LastUpdateDate
		FROM
			MP_ExperianDataCache
		WHERE
			CustomerId = @CustomerId AND 
			Name IS NOT NULL AND
			DirectorId = @DirectorId
	END	
END
GO
