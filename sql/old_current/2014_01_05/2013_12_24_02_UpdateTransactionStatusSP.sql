IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTransactionStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTransactionStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateTransactionStatus] 
(@TrackingId nvarchar(100), @TransactionStatus nvarchar(50), @Description nvarchar(max))

AS
BEGIN

UPDATE [dbo].[LoanTransaction]
   SET [Status] = @TransactionStatus, PacnetStatus=@TransactionStatus, [Description] = @Description
 WHERE TrackingNumber = @TrackingId


END
GO
