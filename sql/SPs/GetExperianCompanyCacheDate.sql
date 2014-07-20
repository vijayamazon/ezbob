IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianCompanyCacheDate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianCompanyCacheDate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianCompanyCacheDate]
	(@CustomerId INT)
AS
BEGIN
	DECLARE @NonLimitedUpdateDate DATETIME
	
	SELECT @NonLimitedUpdateDate = Created FROM ExperianNonLimitedResults WHERE CustomerId = @CustomerId AND IsActive = 1

	IF @NonLimitedUpdateDate IS NOT NULL
	BEGIN
		SELECT @NonLimitedUpdateDate AS LastUpdateDate
	END
	ELSE
	BEGIN
		SELECT
			MIN(LastUpdateDate) AS LastUpdateDate
		FROM
			MP_ExperianDataCache
		WHERE
			CustomerId = @CustomerId AND 
			Name IS NULL AND
			CompanyRefNumber IS NOT NULL
	END
END
GO
