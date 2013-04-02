ALTER TABLE dbo.MP_TeraPeakOrderItem ADD
	RangeMarker int NOT NULL CONSTRAINT DF_MP_TeraPeakOrderItem_RangeMarket DEFAULT 0

GO
DECLARE @v sql_variant 
SET @v = N'0 - Full Range
1- Partial Filled
2 -Temporary'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'dbo', N'TABLE', N'MP_TeraPeakOrderItem', N'COLUMN', N'RangeMarker'
GO
