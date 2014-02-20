IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPersonalInfoForBankBasedApproval]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPersonalInfoForBankBasedApproval]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPersonalInfoForBankBasedApproval] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE 
		@AmlId BIGINT,
		@FirstName NVARCHAR(250),
		@LastName NVARCHAR(250)
		
	SELECT TOP 1
		@AmlId = Id
	FROM
		MP_ServiceLog
	WHERE
		CustomerId = @CustomerId AND
		ServiceType = 'AML A check'
	ORDER BY
		InsertDate DESC
				
	SELECT
		@FirstName = FirstName,
		@LastName = Surname
	FROM
		Customer
	WHERE
		Id = @CustomerId
	


	SELECT
		(SELECT ResponseData FROM MP_ServiceLog WHERE Id = @AmlId) AS AmlData,
		@FirstName AS FirstName,
		@LastName AS Surname--,
		--(SELECT JsonPacket FROM MP_ExperianDataCache WHERE Id = @CompanyId) AS CompanyData
		
	
	
	
END
GO
