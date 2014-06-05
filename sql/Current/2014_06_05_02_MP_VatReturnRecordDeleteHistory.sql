IF OBJECT_ID('MP_VatReturnRecordDeleteHistory') IS NULL
BEGIN
	CREATE TABLE MP_VatReturnRecordDeleteHistory (
		HistoryItemID BIGINT IDENTITY(1, 1) NOT NULL,
		DeletedRecordID INT NOT NULL,
		ReasonRecordID INT NULL,
		ReasonID INT NOT NULL,
		DeletedTime DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_VatReturnRecordDeleteHistory PRIMARY KEY (HistoryItemID),
		CONSTRAINT FK_VatReturnRecordDeleteHistory_DeletedRec FOREIGN KEY (DeletedRecordID) REFERENCES MP_VatReturnRecords (Id),
		CONSTRAINT FK_VatReturnRecordDeleteHistory_ReasonRec FOREIGN KEY (ReasonRecordID) REFERENCES MP_VatReturnRecords (Id),
		CONSTRAINT FK_VatReturnRecordDeleteHistory_Reason FOREIGN KEY (ReasonID) REFERENCES MP_VatReturnRecordDeleteReason (ReasonID)
	)
END
GO
