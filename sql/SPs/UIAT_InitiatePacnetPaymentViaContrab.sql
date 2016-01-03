SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UIAT_InitiatePacnetPaymentViaContrab') IS NULL
	EXECUTE('CREATE PROCEDURE UIAT_InitiatePacnetPaymentViaContrab AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[UIAT_InitiatePacnetPaymentViaContrab]
AS
BEGIN
	UPDATE [ezbob].[dbo].[EzServiceCrontab] SET RepetitionTime=DATEADD(minute,1,GETDATE()) 
	WHERE JobID=(SELECT TOP 1 JobID FROM [ezbob].[dbo].[EzServiceCrontab] WHERE ActionNameID=221 ORDER BY JobID)
END