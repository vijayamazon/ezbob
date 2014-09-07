IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShouldStopSendingLateMails]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[ShouldStopSendingLateMails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ShouldStopSendingLateMails] 
	(@LoanId INT)
AS
BEGIN
	DECLARE @StopSendingEmails BIT
	SELECT @StopSendingEmails = StopSendingEmails FROM LoanOptions WHERE LoanId = @LoanId

	IF @StopSendingEmails IS NULL 
		SET @StopSendingEmails = 0

	SELECT CAST(CASE WHEN @StopSendingEmails = 1 THEN 1 ELSE 0 END AS BIT) AS StopSendingEmails
END
GO
