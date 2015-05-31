
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PaypointTransactionStatusesLoad') IS NOT NULL
	DROP PROCEDURE NL_PaypointTransactionStatusesLoad;
GO


CREATE PROCEDURE [dbo].[NL_PaypointTransactionStatusesLoad]
AS
BEGIN

	SET NOCOUNT ON;
  
	SELECT [PaypointTransactionStatusID], [TransactionStatus] from [dbo].[NL_PaypointTransactionStatuses];
END
GO
