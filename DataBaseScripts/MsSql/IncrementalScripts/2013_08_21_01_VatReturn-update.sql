IF OBJECT_ID('MP_VatReturnRecords') IS NOT NULL
BEGIN
	ALTER TABLE MP_VatReturnRecords DROP CONSTRAINT DF_VatReturnRecord_RegNo

	ALTER TABLE MP_VatReturnRecords DROP COLUMN RegistrationNo

	ALTER TABLE MP_VatReturnRecords ADD RegistrationNo BIGINT NOT NULL CONSTRAINT DF_VatReturnRecord_RegNo DEFAULT (0)
END
GO
