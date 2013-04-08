IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE GetExperianData
	@FirstIdToHandle BIGINT, -- Will handle the one after this
	@LastIdToHandle BIGINT
AS
BEGIN
	SELECT 
		Id, 
		ServiceType, 
		InsertDate, 
		RequestData, 
		ResponseData, 
		CustomerId 
	FROM 
		MP_ServiceLog 
	WHERE 
		ServiceType = 'Consumer Request' AND
		Id > @FirstIdToHandle AND
		Id <= @LastIdToHandle
	ORDER BY
		Id ASC
END
GO
