IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItem2]') AND type IN (N'U'))
BEGIN
	EXEC sp_rename MP_AmazonOrderItem2Payment, MP_AmazonOrderItemPayment
	EXEC sp_rename MP_AmazonOrderItem2, MP_AmazonOrderItem
END 
GO

if exists(select * from sys.columns where Name = N'OrderItem2Id' and Object_ID = Object_ID(N'MP_AmazonOrderItemPayment'))
BEGIN
	EXEC sp_rename 'MP_AmazonOrderItemPayment.OrderItem2Id', 'OrderItemId', 'COLUMN'
END
GO

if exists(select * from sys.columns where Name = N'OrderItem2Id' and Object_ID = Object_ID(N'MP_AmazonOrderItemDetail'))
BEGIN
	EXEC sp_rename 'MP_AmazonOrderItemDetail.OrderItem2Id', 'OrderItemId', 'COLUMN'
END
GO
