IF OBJECT_ID (N'dbo.CustomerLoyaltyProgramPoints') IS NOT NULL
	DROP VIEW dbo.CustomerLoyaltyProgramPoints
GO

CREATE VIEW [dbo].[CustomerLoyaltyProgramPoints]
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

