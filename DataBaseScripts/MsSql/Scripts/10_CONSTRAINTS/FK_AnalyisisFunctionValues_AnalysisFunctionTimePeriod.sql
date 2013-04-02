IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AnalyisisFunctionValues_AnalysisFunctionTimePeriod]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AnalyisisFunctionValues]'))
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues] DROP CONSTRAINT [FK_AnalyisisFunctionValues_AnalysisFunctionTimePeriod]
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues]  WITH CHECK ADD  CONSTRAINT [FK_AnalyisisFunctionValues_AnalysisFunctionTimePeriod] FOREIGN KEY([AnalysisFunctionTimePeriodId])
REFERENCES [dbo].[MP_AnalysisFunctionTimePeriod] ([Id])
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues] CHECK CONSTRAINT [FK_AnalyisisFunctionValues_AnalysisFunctionTimePeriod]
GO
