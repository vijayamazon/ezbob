IF OBJECT_ID('UpdateMainStratFinishDate') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMainStratFinishDate AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMainStratFinishDate 
@CustomerID int,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[Customer] SET
		[LastStartedMainStrategyEndTime] = @Now
	WHERE
		Id = @CustomerID
END
GO
