IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem]') AND type in (N'U'))
BEGIN
DROP TABLE MP_PayPalTransactionItem
END


IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItem2Backup]') AND type in (N'U'))
BEGIN
DROP TABLE MP_AmazonOrderItem2Backup
END 

