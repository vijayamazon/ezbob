IF not EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_AnalyisisFunctionValues]') AND name = N'IX_MP_AnalyisisFunctionValues_Include')

CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_Include] ON [dbo].[MP_AnalyisisFunctionValues] 
(
 [AnalyisisFunctionId] ASC
)
INCLUDE ( [AnalysisFunctionTimePeriodId],
[ValueInt],
[ValueFloat],
[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO