IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateSignalsSelectingStatus]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateSignalsSelectingStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateSignalsSelectingStatus]
AS
BEGIN
	update Signal set Status = 1 where Status=-1
END
GO
