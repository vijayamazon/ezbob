SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'WinMailTemplateName')
BEGIN
	ALTER TABLE Lotteries DROP COLUMN TimestampCounter

	ALTER TABLE Lotteries ADD WinMailTemplateName NVARCHAR(255) NULL

	ALTER TABLE Lotteries ADD TimestampCounter ROWVERSION
END
GO
