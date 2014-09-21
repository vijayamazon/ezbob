IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetZooplaIntEstimate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetZooplaIntEstimate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SetZooplaIntEstimate]
	(@Id INT, @IntValue INT)
AS
BEGIN
	UPDATE Zoopla SET ZooplaEstimateValue = @IntValue WHERE Id = @Id
END
GO
