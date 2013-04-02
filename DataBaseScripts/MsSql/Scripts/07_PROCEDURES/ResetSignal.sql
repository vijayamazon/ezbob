IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetSignal]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ResetSignal]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE ResetSignal
	@pSignalId INT
AS
BEGIN
	UPDATE Signal
	SET    STATUS = 0
	WHERE  id = @pSignalId;
END;
GO
