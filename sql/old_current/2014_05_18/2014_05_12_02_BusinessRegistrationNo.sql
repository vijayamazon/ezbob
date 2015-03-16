IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'RegistrationNo' AND id = OBJECT_ID('Business'))
	ALTER TABLE Business ADD RegistrationNo BIGINT NULL
GO

UPDATE Business SET
	RegistrationNo = r.RegistrationNo
FROM
	Business b
	INNER JOIN MP_VatReturnRecords r ON b.Id = r.BusinessId
GO
