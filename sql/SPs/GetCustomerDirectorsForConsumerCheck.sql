IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerDirectorsForConsumerCheck]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerDirectorsForConsumerCheck]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomerDirectorsForConsumerCheck] 
	(@CustomerId INT)
AS
BEGIN
	SELECT 
		d.id AS DirId,
		d.Name AS DirName, 
		d.Surname AS DirSurname
	FROM 
		Director d
		LEFT JOIN CustomerAddress ca ON ca.DirectorId = d.Id
	WHERE
		d.CustomerId = @CustomerId AND
		(
			ca.addressType = '4' OR 
			ca.addressType = '6'
		)
END
GO
