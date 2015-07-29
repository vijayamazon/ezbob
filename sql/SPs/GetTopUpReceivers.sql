IF OBJECT_ID('GetTopUpReceivers') IS NULL
	EXECUTE('CREATE PROCEDURE GetTopUpReceivers AS SELECT 1')
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetTopUpReceivers]
AS
BEGIN
	SELECT 
		Email,
		SendMobilePhone,
		PhoneOriginIsrael
	FROM
		TopUpReceiver
	WHERE
		IsActive = 1
END 
GO

















