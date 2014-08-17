IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPersonalInfoForConsumerCheck]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPersonalInfoForConsumerCheck]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPersonalInfoForConsumerCheck] 
	(@CustomerId INT,
	 @DirectorId INT = NULL)
AS
BEGIN
	IF @DirectorId IS NULL
	BEGIN
		SELECT
			FirstName, 
			Surname,
			Gender,
			DateOfBirth,		
			TimeAtAddress
		FROM
			Customer
		WHERE
			Id = @CustomerId
	END
	ELSE
	BEGIN
		SELECT
			Name AS FirstName, 
			Surname,
			Gender,
			DateOfBirth,
			Id AS TimeAtAddress
		FROM
			Director
		WHERE
			Id = @DirectorId
	END
END
GO
