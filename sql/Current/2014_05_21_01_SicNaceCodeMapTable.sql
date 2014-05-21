IF OBJECT_ID('SicNaceCodeMap') IS NULL
BEGIN
	CREATE TABLE SicNaceCodeMap
	(
		Id INT IDENTITY(1,1) NOT NULL,
		SicFirstTwoDigits NVARCHAR(2) NOT NULL,
		NaceCode NVARCHAR(3) NOT NULL,
		Description NVARCHAR(300),
		CONSTRAINT PK_SicNaceCodeMap PRIMARY KEY (Id),
		
	)
END
GO

IF (SELECT COUNT(*) FROM SicNaceCodeMap) = 0
BEGIN

INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '01', 'A01', 'Agriculture, Forestry and Fishing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '02', 'A02', 'Agriculture, Forestry and Fishing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '05', 'A03', 'Agriculture, Forestry and Fishing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '10', 'B05', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '11', 'B06', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '12', 'B07', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '13', 'B08', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '14', 'B09', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '15', 'C10', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '16', 'C11', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '17', 'C12', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '18', 'C13', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '19', 'C14', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '20', 'C15', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '21', 'C16', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '22', 'C17', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '23', 'C18', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '24', 'C19', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '25', 'C20', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '26', 'C21', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '27', 'C22', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '28', 'C23', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '29', 'C24', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '30', 'C25', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '31', 'C26', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '32', 'C27', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '33', 'C28', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '34', 'C29', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '35', 'C30', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '36', 'C31', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '37', 'C32', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '40', 'D35', 'Electricity, Gas, Steam and Air Conditioning')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '41', 'E36', 'Water Supply, Sewerage, Waste Management and Remediation Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '45', 'F41', 'Construction')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '50', 'G45', 'Wholesale and Retail Trade; Repair of Motor Vehicles and Motorcycles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '51', 'G46', 'Wholesale and Retail Trade; Repair of Motor Vehicles and Motorcycles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '52', 'G47', 'Wholesale and Retail Trade; Repair of Motor Vehicles and Motorcycles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '55', 'I55', 'Accomodation and Food Service Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '60', 'H49', 'Transportation and Storage')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '61', 'H50', 'Transportation and Storage')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '62', 'H51', 'Transportation and Storage')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '63', 'H52', 'Transportation and Storage')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '64', 'H53', 'Postal and courier activities ')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '65', 'K64', 'Financial and Insurance Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '66', 'K65', 'Financial and Insurance Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '67', 'K66', 'Financial and Insurance Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '70', 'L68', 'Real Estate Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '71', 'M69', 'Professional, Scientific and Technical Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '72', 'M70', 'Professional, Scientific and Technical Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '73', 'M71', 'Professional, Scientific and Technical Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '74', 'N82', 'Administrative and Support Service Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '75', 'O84', 'Public Administration and Defence; Compulsory Social Security')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '80', 'P85', 'Education')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '85', 'Q86', 'Human Health and Social Work Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '90', 'R90', 'Arts, Entertainment and Recreation')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '91', 'R91', 'Arts, Entertainment and Recreation')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '92', 'R92', 'Arts, Entertainment and Recreation')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '93', 'S96', 'Other Service Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '95', 'T97', 'Activities of Households as Employers; Undifferentiated Goods- and Services- Producing Activities of Households for Own Use')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '99', 'U99', 'Activities of Extraterritorial Organisations and Bodies')

END
GO
