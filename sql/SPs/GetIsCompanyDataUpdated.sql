IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIsCompanyDataUpdated]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetIsCompanyDataUpdated]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetIsCompanyDataUpdated] 
	(@CompanyRefNumber NVARCHAR(50))
AS
BEGIN
	DECLARE 
		@Today DATE,
		@LastUpdateTime DATE
	
	SELECT @Today = getutcdate()
		
	SELECT @LastUpdateTime = LastUpdateDate FROM MP_ExperianDataCache WHERE CompanyRefNumber = @CompanyRefNumber 
	
	IF @Today = @LastUpdateTime
		SELECT CAST (1 AS BIT) AS IsUpdated
	ELSE
		SELECT CAST (0 AS BIT) AS IsUpdated
END
GO
