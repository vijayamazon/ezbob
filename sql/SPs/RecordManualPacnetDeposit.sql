IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecordManualPacnetDeposit]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RecordManualPacnetDeposit]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RecordManualPacnetDeposit]
	(@UnderwriterName VARCHAR(100),
	 @Amount INT,
	 @Date DATETIME)
AS
BEGIN
	INSERT INTO PacNetManualBalance	(Username, Amount, [Date], Enabled) VALUES (@UnderwriterName, @Amount, @Date, 1)
END
GO
