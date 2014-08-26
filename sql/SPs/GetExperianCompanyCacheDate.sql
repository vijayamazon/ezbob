IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianCompanyCacheDate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianCompanyCacheDate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianCompanyCacheDate]
	(@RefNumber NVARCHAR(50))
AS
BEGIN
	DECLARE @NonLimitedUpdateDate DATETIME
	
	SELECT @NonLimitedUpdateDate = Created FROM ExperianNonLimitedResults WHERE RefNumber = @RefNumber AND IsActive = 1

	IF @NonLimitedUpdateDate IS NOT NULL
	BEGIN
		SELECT @NonLimitedUpdateDate AS LastUpdateDate
	END
	ELSE
	BEGIN	
		DECLARE @ServiceLogId BIGINT
		
		SELECT TOP 1
			@ServiceLogId = Id
		FROM
			MP_ServiceLog
		WHERE
			CompanyRefNum = @RefNumber AND
			ServiceType = 'E-SeriesLimitedData'
		ORDER BY
			Id DESC	
	
		SELECT InsertDate AS LastUpdateDate FROM MP_ServiceLog WHERE Id = @ServiceLogId
	END
END

GO