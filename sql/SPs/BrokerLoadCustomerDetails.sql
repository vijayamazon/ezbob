IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrokerLoadCustomerDetails]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[BrokerLoadCustomerDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BrokerLoadCustomerDetails] 
	(@CustomerID INT,
@ContactEmail NVARCHAR(255))
AS
BEGIN
	DECLARE @BrokerID INT

	SELECT @BrokerID = BrokerID FROM Broker WHERE ContactEmail = @ContactEmail

	SELECT
		c.Id AS CustomerID,
		c.FirstName,
		c.Surname,
		c.DateOfBirth,
		c.Gender,
		c.MaritalStatus,
		c.MobilePhone,
		c.DaytimePhone,
		a.Organisation,
		a.Line1,
		a.Line2,
		a.Line3,
		a.Pobox,
		a.Town,
		a.County,
		a.Postcode,
		a.Country
	FROM
		Customer c
		LEFT JOIN CustomerAddress a ON c.Id = a.CustomerId AND a.addressType = 1
	WHERE
		c.Id = @CustomerID
		AND
		c.BrokerID = @BrokerID
END
GO
