SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UIAT_InitiatePacnetPaymentViaContrab') IS NULL
	EXECUTE('CREATE PROCEDURE UIAT_InitiatePacnetPaymentViaContrab AS SELECT 1')
GO

ALTER PROCEDURE UIAT_InitiatePacnetPaymentViaContrab
AS
BEGIN
	UPDATE EzServiceCrontab SET RepetitionTime=DATEADD(minute,1,GETDATE()) 
	WHERE ActionNameID=(SELECT TOP 1 ActionNameID FROM EzServiceActionName WHERE ActionName LIKE '%BrokerTransferCommission' ORDER BY JobID)
END
GO