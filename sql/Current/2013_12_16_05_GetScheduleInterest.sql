IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetScheduleInterest]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetScheduleInterest]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetScheduleInterest]
	(@LoanSchedule INT) 
AS
BEGIN
	SELECT 
		ls.Interest
	FROM 
		LoanSchedule ls 
	WHERE 
		id = @LoanSchedule
END
GO
