IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLastNotice]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateLastNotice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLastNotice]
	@LoanScheduleId INT
AS
BEGIN
	UPDATE
		LoanSchedule
	SET
		LastNoticeSent = 1
	WHERE
		Id = @LoanScheduleId
END
GO
