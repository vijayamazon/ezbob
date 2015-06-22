SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PacnetTransactionStatusesLoad') IS NOT NULL
	DROP PROCEDURE NL_PacnetTransactionStatusesLoad;
GO


CREATE PROCEDURE [dbo].[NL_PacnetTransactionStatusesLoad]
AS
BEGIN
	
	SET NOCOUNT ON;
  
	SELECT PacnetTransactionStatusID, TransactionStatus from NL_PacnetTransactionStatuses;
END
