IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AnalyisisFunctionValues_AnalyisisFunction]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AnalyisisFunctionValues]'))
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues] DROP CONSTRAINT [FK_AnalyisisFunctionValues_AnalyisisFunction]
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues]  WITH CHECK ADD  CONSTRAINT [FK_AnalyisisFunctionValues_AnalyisisFunction] FOREIGN KEY([AnalyisisFunctionId])
REFERENCES [dbo].[MP_AnalyisisFunction] ([Id])
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues] CHECK CONSTRAINT [FK_AnalyisisFunctionValues_AnalyisisFunction]
GO
