IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllConsumersForBackfill]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllConsumersForBackfill]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllConsumersForBackfill] 
AS
BEGIN
	SELECT 
		Customer.Id AS CustomerId,
		MP_ExperianDataCache.DirectorId,
		Customer.FirstName,
		Customer.Surname,
		Customer.Gender,
		Customer.DateOfBirth		
	FROM 
		Customer,
		MP_ExperianDataCache
	WHERE
		Customer.Id = MP_ExperianDataCache.CustomerId
END
GO
