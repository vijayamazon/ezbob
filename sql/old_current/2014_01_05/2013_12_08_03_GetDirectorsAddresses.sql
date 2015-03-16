IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDirectorsAddresses]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetDirectorsAddresses]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetDirectorsAddresses] 
	(@CustomerId INT)
AS
BEGIN
	SELECT 
		d.id AS DirId, 
		ca.addressType AS AddressType, 
		ca.Line1 AS DirLine1, 
		ca.Line2 AS DirLine2, 
		ca.Line3 AS DirLine3, 
		ca.Town AS DirLine4 , 
		ca.County AS DirLine5, 
		ca.Postcode AS DirLine6,
		d.Name AS DirName, 
		d.Surname AS DirSurname, 
		d.DateOfBirth AS DirDateOfBirth, 
		d.Gender AS DirGender
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
