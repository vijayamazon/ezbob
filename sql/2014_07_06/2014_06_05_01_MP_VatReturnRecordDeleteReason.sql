IF OBJECT_ID('MP_VatReturnRecordDeleteReason') IS NULL
BEGIN
	CREATE TABLE MP_VatReturnRecordDeleteReason (
		ReasonID INT NOT NULL,
		Reason NVARCHAR(128) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_VatReturnRecordDeleteReason PRIMARY KEY (ReasonID),
		CONSTRAINT CHK_VatReturnRecordDeleteReason CHECK(LTRIM(RTRIM(Reason)) != ''),
		CONSTRAINT UC_VatReturnRecordDeleteReason UNIQUE (Reason)
	)

	INSERT INTO MP_VatReturnRecordDeleteReason(ReasonID, Reason) VALUES
		(1, 'Uploaded equal'),
		(2, 'Uploaded not equal'),
		(3, 'Manual overridden by uploaded'),
		(4, 'Manual updated'),
		(5, 'Manual deleted'),
		(6, 'Manual rejected because of uploaded'),
		(7, 'Manual rejected because of linked'),
		(8, 'Uploaded rejected because of linked'),
		(9, 'Linked updated')
END
GO
