IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MP_ChannelGrabberOrderItem') AND name = 'IsExpense')
	ALTER TABLE MP_ChannelGrabberOrderItem ADD IsExpense INT NOT NULL CONSTRAINT DF_ChannelGrabberOrderItem_Expense DEFAULT (0)
GO
