IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAvailableFunds]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAvailableFunds]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAvailableFunds]
AS
BEGIN
	DECLARE 
		@LastReportDate DATETIME,
		@ManualBalance INT,
		@AdjustedBalance DECIMAL(18,4)

	SELECT @LastReportDate = Date, @AdjustedBalance = Adjusted FROM fnPacnetBalance()

	IF @LastReportDate IS NOT NULL
		SELECT @LastReportDate = DATEADD(dd, 1, @LastReportDate)

	SELECT @ManualBalance = SUM(Amount) FROM PacNetManualBalance WHERE Enabled = 1 AND (@LastReportDate IS NULL OR [Date] >= @LastReportDate)

	SELECT @AdjustedBalance + ISNULL(@ManualBalance, 0) AS AvailableFunds
END
GO
