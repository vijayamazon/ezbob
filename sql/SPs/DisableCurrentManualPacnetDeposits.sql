IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DisableCurrentManualPacnetDeposits]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DisableCurrentManualPacnetDeposits]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DisableCurrentManualPacnetDeposits]
AS
BEGIN
	UPDATE PacNetManualBalance Set Enabled = 0 WHERE Enabled = 1	
END
GO
