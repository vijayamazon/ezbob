IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoanStats_CustomerPostcodes]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptLoanStats_CustomerPostcodes]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptLoanStats_CustomerPostcodes
AS
SELECT
	c.Id AS CustomerID,
	a.Rawpostcode
FROM
	Customer c
	INNER JOIN CustomerAddress a
		ON c.Id = a.CustomerId
		AND a.addressType = 1
WHERE
	c.IsTest = 0
GO
