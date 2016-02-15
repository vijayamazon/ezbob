SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_AmazonOrderItemDetail_missing_549')
	Create NonClustered Index IX_MP_AmazonOrderItemDetail_missing_549 On [ezbob].[dbo].[MP_AmazonOrderItemDetail] ([OrderItemId])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EBayOrderItemDetail_missing_208')
	Create NonClustered Index IX_MP_EBayOrderItemDetail_missing_208 On [ezbob].[dbo].[MP_EBayOrderItemDetail] ([ItemId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_PayPalTransactionItem2_missing_71')
	Create NonClustered Index IX_MP_PayPalTransactionItem2_missing_71 On [ezbob].[dbo].[MP_PayPalTransactionItem2] ([TransactionId]) Include ([Id], [Created], [CurrencyId], [FeeAmount], [GrossAmount], [NetAmount], [TimeZone], [Type], [Status], [PayPalTransactionId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_AmazonFeedbackItem_missing_111')
	Create NonClustered Index IX_MP_AmazonFeedbackItem_missing_111 On [ezbob].[dbo].[MP_AmazonFeedbackItem] ([TimePeriodId]) Include ([AmazonFeedbackId], [Positive]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EbayExternalTransaction_missing_587')
	Create NonClustered Index IX_MP_EbayExternalTransaction_missing_587 On [ezbob].[dbo].[MP_EbayExternalTransaction] ([OrderItemId]) Include ([Id], [TransactionID], [TransactionTime], [FeeOrCreditCurrency], [FeeOrCreditPrice], [PaymentOrRefundACurrency], [PaymentOrRefundAPrice]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_missing_418')
	Create NonClustered Index IX_MP_AnalyisisFunctionValues_missing_418 On [ezbob].[dbo].[MP_AnalyisisFunctionValues] ([AnalysisFunctionTimePeriodId],[AnalyisisFunctionId]) Include ([Updated], [CustomerMarketPlaceId], [ValueFloat]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_YodleeOrderItemBankTransaction_missing_600')
	Create NonClustered Index IX_MP_YodleeOrderItemBankTransaction_missing_600 On [ezbob].[dbo].[MP_YodleeOrderItemBankTransaction] ([transactionAmount], [description]) Include ([OrderItemId], [transactionDate], [postDate]);
GO
