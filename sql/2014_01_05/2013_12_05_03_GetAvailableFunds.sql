IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAvailableFunds]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAvailableFunds]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAvailableFunds] 
AS
BEGIN
	DECLARE @AvailableFunds FLOAT,
			@AvailableFundsDate DATE,
			@ManualFundsAdded FLOAT

	SELECT 
		@AvailableFundsDate = DATE, 
		@AvailableFunds = Adjusted 
	FROM 
		fnPacnetBalance()

	 
	SELECT 
		@ManualFundsAdded = Sum(Amount) 
	FROM 
		PacNetManualBalance pnb
	WHERE 
		@AvailableFundsDate <= pnb.Date AND 
		pnb.Enabled = 1

	SELECT isnull(@AvailableFunds,0) + isnull(@ManualFundsAdded,0) AS AvailableFunds
END
GO
