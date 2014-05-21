IF OBJECT_ID('SicNaceCodeMap') IS NULL
BEGIN
	CREATE TABLE SicNaceCodeMap
	(
		Id INT IDENTITY(1,1) NOT NULL,
		SicFirstTwoDigits NVARCHAR(2) NOT NULL,
		NaceCode NVARCHAR(1) NOT NULL,
		Description NVARCHAR(300),
		CONSTRAINT PK_SicNaceCodeMap PRIMARY KEY (Id),
		
	)
END
GO

IF (SELECT COUNT(*) FROM SicNaceCodeMap) = 0
BEGIN

INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '01', 'A', 'Agriculture, Forestry and Fishing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '02', 'A', 'Agriculture, Forestry and Fishing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '05', 'A', 'Agriculture, Forestry and Fishing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '10', 'B', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '11', 'B', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '12', 'B', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '13', 'B', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '14', 'B', 'Mining and Quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '15', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '16', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '17', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '18', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '19', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '20', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '21', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '22', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '23', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '24', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '25', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '26', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '27', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '28', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '29', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '30', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '31', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '32', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '33', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '34', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '35', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '36', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '37', 'C', 'Manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '40', 'D', 'Electricity, Gas, Steam and Air Conditioning')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '41', 'E', 'Water Supply, Sewerage, Waste Management and Remediation Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '45', 'F', 'Construction')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '50', 'G', 'Wholesale and Retail Trade; Repair of Motor Vehicles and Motorcycles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '51', 'G', 'Wholesale and Retail Trade; Repair of Motor Vehicles and Motorcycles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '52', 'G', 'Wholesale and Retail Trade; Repair of Motor Vehicles and Motorcycles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '55', 'I', 'Accomodation and Food Service Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '60', 'H', 'Transportation and Storage')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '61', 'H', 'Transportation and Storage')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '62', 'H', 'Transportation and Storage')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '63', 'H', 'Transportation and Storage')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '64', 'I', 'Accomodation and Food Service Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '65', 'K', 'Financial and Insurance Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '66', 'K', 'Financial and Insurance Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '67', 'K', 'Financial and Insurance Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '70', 'L', 'Real Estate Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '71', 'M', 'Professional, Scientific and Technical Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '72', 'M', 'Professional, Scientific and Technical Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '73', 'M', 'Professional, Scientific and Technical Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '74', 'N', 'Administrative and Support Service Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '75', 'O', 'Public Administration and Defence; Compulsory Social Security')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '80', 'P', 'Education')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '85', 'Q', 'Human Health and Social Work Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '90', 'R', 'Arts, Entertainment and Recreation')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '91', 'R', 'Arts, Entertainment and Recreation')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '92', 'R', 'Arts, Entertainment and Recreation')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '93', 'S', 'Other Service Activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '95', 'T', 'Activities of Households as Employers; Undifferentiated Goods- and Services- Producing Activities of Households for Own Use')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Description) VALUES ( '99', 'U', 'Activities of Extraterritorial Organisations and Bodies')

END
GO
