IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AnalyisisFunction_ValueType]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AnalyisisFunction]'))
ALTER TABLE [dbo].[MP_AnalyisisFunction] DROP CONSTRAINT [FK_AnalyisisFunction_ValueType]
GO
ALTER TABLE [dbo].[MP_AnalyisisFunction]  WITH CHECK ADD  CONSTRAINT [FK_AnalyisisFunction_ValueType] FOREIGN KEY([ValueTypeId])
REFERENCES [dbo].[MP_ValueType] ([Id])
GO
ALTER TABLE [dbo].[MP_AnalyisisFunction] CHECK CONSTRAINT [FK_AnalyisisFunction_ValueType]
GO
