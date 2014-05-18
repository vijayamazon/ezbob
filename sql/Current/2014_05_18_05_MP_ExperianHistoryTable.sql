IF OBJECT_ID('MP_ExperianHistory') IS NULL
BEGIN
	CREATE TABLE MP_ExperianHistory
	(
		Id INT IDENTITY(1,1),
		CustomerId INT NOT NULL,
		Type NVARCHAR(30),
		Date DATETIME NOT NULL,
		ServiceLogId BIGINT NOT NULL,
		Score INT,
		CII INT,
		CONSTRAINT PK_MP_ExperianHistory PRIMARY KEY (Id),
		CONSTRAINT FK_MP_ExperianHistory_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customer(Id),
		CONSTRAINT FK_MP_ExperianHistory_MP_ServiceLog FOREIGN KEY (ServiceLogId) REFERENCES MP_ServiceLog(Id)
	)
END
GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_MP_ExperianHistory_CustomerType')
	CREATE NONCLUSTERED INDEX IX_MP_ExperianHistory_CustomerType ON MP_ExperianHistory(CustomerId, Type)
GO
