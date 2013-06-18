IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_TeraPeakCategoryStatistics_MP_TeraPeakCategory]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_TeraPeakCategoryStatistics]'))
ALTER TABLE [dbo].[MP_TeraPeakCategoryStatistics] DROP CONSTRAINT [FK_MP_TeraPeakCategoryStatistics_MP_TeraPeakCategory]
GO
ALTER TABLE [dbo].[MP_TeraPeakCategoryStatistics]  WITH CHECK ADD  CONSTRAINT [FK_MP_TeraPeakCategoryStatistics_MP_TeraPeakCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[MP_TeraPeakCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_TeraPeakCategoryStatistics] CHECK CONSTRAINT [FK_MP_TeraPeakCategoryStatistics_MP_TeraPeakCategory]
GO
