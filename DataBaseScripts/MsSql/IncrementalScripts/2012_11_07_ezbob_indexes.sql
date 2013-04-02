/****** Object:  Index [IX_MP_PayPalTransactionItem_Type]    Script Date: 11/07/2012 17:42:40 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem]') AND name = N'IX_MP_PayPalTransactionItem_Type')
DROP INDEX [IX_MP_PayPalTransactionItem_Type] ON [dbo].[MP_PayPalTransactionItem] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_MP_PayPalTransactionItem_Type]    Script Date: 11/07/2012 17:42:40 ******/
CREATE NONCLUSTERED INDEX [IX_MP_PayPalTransactionItem_Type] ON [dbo].[MP_PayPalTransactionItem] 
(
	[TransactionId] ASC,
	[Type] ASC,
	[Status] ASC,
	[NetAmountAmount] ASC
)
INCLUDE ( [Payer],
[Created],
[FeeAmountCurrency],
[FeeAmountAmount],
[GrossAmountCurrency],
[GrossAmountAmount],
[NetAmountCurrency],
[TimeZone],
[PayerDisplayName],
[PayPalTransactionId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO


/****** Object:  Index [IX_Application_HistorySumm]    Script Date: 11/07/2012 17:50:40 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Application_HistorySumm]') AND name = N'IX_Application_HistorySumm')
DROP INDEX [IX_Application_HistorySumm] ON [dbo].[Application_HistorySumm] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_Application_HistorySumm]    Script Date: 11/07/2012 17:50:40 ******/
CREATE NONCLUSTERED INDEX [IX_Application_HistorySumm] ON [dbo].[Application_HistorySumm] 
(
	[ApplicationId] ASC,
	[NodeId] ASC
)
INCLUDE ( [SecurityApplicationId],
[Id]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO


/****** Object:  Index [i_app_hist_1]    Script Date: 11/07/2012 17:53:06 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Application_History]') AND name = N'i_app_hist_1')
DROP INDEX [i_app_hist_1] ON [dbo].[Application_History] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [i_app_hist_1]    Script Date: 11/07/2012 17:53:07 ******/
CREATE NONCLUSTERED INDEX [i_app_hist_1] ON [dbo].[Application_History] 
(
	[UserId] ASC
)
INCLUDE ( [AppHistoryId],
[ActionDateTime],
[CurrentNodeID],
[ActionType],
[ApplicationId],
[SecurityApplicationId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

/****** Object:  Index [i_se_es_1]    Script Date: 11/07/2012 18:00:55 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[StrategyEngine_ExecutionState]') AND name = N'i_se_es_1')
DROP INDEX [i_se_es_1] ON [dbo].[StrategyEngine_ExecutionState] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [i_se_es_1]    Script Date: 11/07/2012 18:00:56 ******/
CREATE NONCLUSTERED INDEX [i_se_es_1] ON [dbo].[StrategyEngine_ExecutionState] 
(
	[ApplicationId] ASC
)
INCLUDE ( [StartTime],
[CurrentNodeId],
[CurrentNodePostfix],
[Id],
[IsTimeoutReported]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

/****** Object:  Index [IX_MP_AnalyisisFunctionValues_AFTPI]    Script Date: 11/07/2012 18:17:26 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_AnalyisisFunctionValues]') AND name = N'IX_MP_AnalyisisFunctionValues_AFTPI')
DROP INDEX [IX_MP_AnalyisisFunctionValues_AFTPI] ON [dbo].[MP_AnalyisisFunctionValues] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_MP_AnalyisisFunctionValues_AFTPI]    Script Date: 11/07/2012 18:17:26 ******/
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_AFTPI] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[AnalysisFunctionTimePeriodId] ASC
)
INCLUDE ( [AnalyisisFunctionId],
[ValueInt],
[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

/****** Object:  Index [IX_MP_AnalyisisFunctionValues_AFI]    Script Date: 11/07/2012 18:17:42 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_AnalyisisFunctionValues]') AND name = N'IX_MP_AnalyisisFunctionValues_AFI')
DROP INDEX [IX_MP_AnalyisisFunctionValues_AFI] ON [dbo].[MP_AnalyisisFunctionValues] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_MP_AnalyisisFunctionValues_AFI]    Script Date: 11/07/2012 18:17:42 ******/
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_AFI] ON [dbo].[MP_AnalyisisFunctionValues] 
(
	[AnalyisisFunctionId] ASC
)
INCLUDE ( [AnalysisFunctionTimePeriodId],
[ValueFloat],
[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO


/****** Object:  Index [IX_MP_CustomerMarketPlace]    Script Date: 11/07/2012 18:26:18 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlace]') AND name = N'IX_MP_CustomerMarketPlace')
DROP INDEX [IX_MP_CustomerMarketPlace] ON [dbo].[MP_CustomerMarketPlace] WITH ( ONLINE = OFF )
GO


/****** Object:  Index [IX_MP_CustomerMarketPlace]    Script Date: 11/07/2012 18:26:19 ******/
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlace] ON [dbo].[MP_CustomerMarketPlace] 
(
	[CustomerId] ASC
)
INCLUDE ( [MarketPlaceId],
[DisplayName],
[EliminationPassed]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

/****** Object:  Index [IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart]    Script Date: 11/08/2012 11:09:34 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlaceUpdatingHistory]') AND name = N'IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart')
DROP INDEX [IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart] ON [dbo].[MP_CustomerMarketPlaceUpdatingHistory] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart]    Script Date: 11/08/2012 11:09:35 ******/
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart] ON [dbo].[MP_CustomerMarketPlaceUpdatingHistory] 
(
	[UpdatingStart] ASC
)
INCLUDE ( [CustomerMarketPlaceId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

