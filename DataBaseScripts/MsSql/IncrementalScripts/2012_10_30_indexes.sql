/****** Object:  Index [PostcodeServiceLog_CustId]    Script Date: 10/30/2012 10:42:43 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PostcodeServiceLog]') AND name = N'PostcodeServiceLog_CustId')
DROP INDEX [PostcodeServiceLog_CustId] ON [dbo].[PostcodeServiceLog] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [PostcodeServiceLog_CustId]    Script Date: 10/30/2012 10:42:43 ******/
CREATE NONCLUSTERED INDEX [PostcodeServiceLog_CustId] ON [dbo].[PostcodeServiceLog] 
(
	[CustomerId] ASC
)
INCLUDE ( [ErrorMessage],
[InsertDate],
[RequestData],
[RequestType],
[ResponseData],
[Status]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO


/****** Object:  Index [PacnetPaypointServiceLog_CustId]    Script Date: 10/30/2012 10:43:17 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PacnetPaypointServiceLog]') AND name = N'PacnetPaypointServiceLog_CustId')
DROP INDEX [PacnetPaypointServiceLog_CustId] ON [dbo].[PacnetPaypointServiceLog] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [PacnetPaypointServiceLog_CustId]    Script Date: 10/30/2012 10:43:17 ******/
CREATE NONCLUSTERED INDEX [PacnetPaypointServiceLog_CustId] ON [dbo].[PacnetPaypointServiceLog] 
(
	[CustomerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

/****** Object:  Index [MP_AnalyisisFunctionValues_AFTP]    Script Date: 10/30/2012 10:43:51 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_AnalyisisFunctionValues]') AND name = N'MP_AnalyisisFunctionValues_AFTP')
DROP INDEX [MP_AnalyisisFunctionValues_AFTP] ON [dbo].[MP_AnalyisisFunctionValues] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [MP_AnalyisisFunctionValues_AFTP]    Script Date: 10/30/2012 10:43:51 ******/
CREATE NONCLUSTERED INDEX [MP_AnalyisisFunctionValues_AFTP] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[AnalysisFunctionTimePeriodId] ASC
)
INCLUDE ( [AnalyisisFunctionId],
[ValueInt],
[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

/****** Object:  Index [MP_PayPalTransactionItem_TI]    Script Date: 10/30/2012 10:44:22 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem]') AND name = N'MP_PayPalTransactionItem_TI')
DROP INDEX [MP_PayPalTransactionItem_TI] ON [dbo].[MP_PayPalTransactionItem] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [MP_PayPalTransactionItem_TI]    Script Date: 10/30/2012 10:44:22 ******/
CREATE NONCLUSTERED INDEX [MP_PayPalTransactionItem_TI] ON [dbo].[MP_PayPalTransactionItem] 
(
	[TransactionId] ASC
)
INCLUDE ( [Id],
[Created],
[FeeAmountCurrency],
[FeeAmountAmount],
[GrossAmountCurrency],
[GrossAmountAmount],
[NetAmountCurrency],
[NetAmountAmount],
[TimeZone],
[Type],
[Status],
[Payer],
[PayerDisplayName],
[PayPalTransactionId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO