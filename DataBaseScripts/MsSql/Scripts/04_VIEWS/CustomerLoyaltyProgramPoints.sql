IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[CustomerLoyaltyProgramPoints]'))
DROP VIEW [dbo].[CustomerLoyaltyProgramPoints]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW CustomerLoyaltyProgramPoints
AS
SELECT
	CustomerID,
	SUM(EarnedPoints) AS EarnedPoints,
	MAX(ActionDate) AS LastActionDate
FROM
	CustomerLoyaltyProgram
GROUP BY
	CustomerID
GO
