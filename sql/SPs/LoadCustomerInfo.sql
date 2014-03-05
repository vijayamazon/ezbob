IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadCustomerInfo]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadCustomerInfo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LoadCustomerInfo] 
	(@CustomerID INT)
AS
BEGIN
	SELECT
		c.Name AS Email,
		c.FirstName AS FirstName,
		c.Surname AS LastName
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID
END
GO
