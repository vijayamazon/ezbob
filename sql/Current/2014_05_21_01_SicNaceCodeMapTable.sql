IF OBJECT_ID('SicNaceCodeMap') IS NULL
BEGIN
	CREATE TABLE SicNaceCodeMap
	(
		Id INT IDENTITY(1,1) NOT NULL,
		SicFirstTwoDigits NVARCHAR(2) NOT NULL,
		NaceCode NVARCHAR(3) NOT NULL,
		Category NVARCHAR(300),
		Description NVARCHAR(300),
		CONSTRAINT PK_SicNaceCodeMap PRIMARY KEY (Id),
	)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Category' AND id = OBJECT_ID('SicNaceCodeMap'))
BEGIN 
	ALTER TABLE SicNaceCodeMap ADD Category NVARCHAR(300)
	DELETE FROM SicNaceCodeMap
END 	
GO


IF (SELECT COUNT(*) FROM SicNaceCodeMap) = 0
BEGIN
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('01', 'A01', 'Agriculture, Forestry and Fishing', 'Crop and animal production, hunting and related service activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('01', 'A01', 'Agriculture, Forestry and Fishing', 'Crop and animal production, hunting and related service activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('02', 'A02', 'Agriculture, Forestry and Fishing', 'Forestry and logging')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('05', 'A03', 'Agriculture, Forestry and Fishing', 'Fishing and aquaculture')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('10', 'B05', 'Mining and Quarrying', 'Mining of coal and lignite')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('11', 'B06', 'Mining and Quarrying', 'Extraction of crude petroleum and natural gas')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('12', 'B07', 'Mining and Quarrying', 'Mining of metal ores')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('13', 'B08', 'Mining and Quarrying', 'Other mining and quarrying')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('14', 'B09', 'Mining and Quarrying', 'Mining support service activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('15', 'C10', 'Manufacturing', 'Manufacture of food products')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('16', 'C11', 'Manufacturing', 'Manufacture of beverages')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('17', 'C12', 'Manufacturing', 'Manufacture of tobacco products')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('18', 'C13', 'Manufacturing', 'Manufacture of textiles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('19', 'C14', 'Manufacturing', 'Manufacture of wearing apparel')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('20', 'C15', 'Manufacturing', 'Manufacture of leather and related products')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('21', 'C16', 'Manufacturing', 'Manufacture of wood and of products of wood and cork, except furniture; manufacture of articles of straw and plaiting materials')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('22', 'C17', 'Manufacturing', 'Manufacture of paper and paper products')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('23', 'C18', 'Manufacturing', 'Printing and reproduction of recorded media')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('24', 'C19', 'Manufacturing', 'Manufacture of coke and refined petroleum products')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('25', 'C20', 'Manufacturing', 'Manufacture of chemicals and chemical products')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('26', 'C21', 'Manufacturing', 'Manufacture of basic pharmaceutical products and pharmaceutical preparations')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('27', 'C22', 'Manufacturing', 'Manufacture of rubber and plastic products')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('28', 'C23', 'Manufacturing', 'Manufacture of other non metallic mineral products')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('29', 'C24', 'Manufacturing', 'Manufacture of basic metals')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('30', 'C25', 'Manufacturing', 'Manufacture of fabricated metal products, except machinery and equipment')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('31', 'C26', 'Manufacturing', 'Manufacture of computer, electronic and optical products')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('32', 'C27', 'Manufacturing', 'Manufacture of electrical equipment')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('33', 'C28', 'Manufacturing', 'Manufacture of machinery and equipment n.e.c.')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('34', 'C29', 'Manufacturing', 'Manufacture of motor vehicles, trailers and semi trailers')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('35', 'C30', 'Manufacturing', 'Manufacture of other transport equipment')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('36', 'C31', 'Manufacturing', 'Manufacture of furniture')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('37', 'C32', 'Manufacturing', 'Other manufacturing')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('40', 'D35', 'Electricity, Gas, Steam and Air Conditioning', 'Electricity, gas, steam and air conditioning supply')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('41', 'E36', 'Water Supply, Sewerage, Waste Management and Remediation Activities', 'Water collection, treatment and supply')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('45', 'F41', 'Construction', 'Construction of buildings')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('50', 'G45', 'Wholesale and Retail Trade; Repair of Motor Vehicles and Motorcycles', 'Wholesale and retail trade and repair of motor vehicles and motorcycles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('51', 'G46', 'Wholesale and Retail Trade; Repair of Motor Vehicles and Motorcycles', 'Wholesale trade, except of motor vehicles and motorcycles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('52', 'G47', 'Wholesale and Retail Trade; Repair of Motor Vehicles and Motorcycles', 'Retail trade, except of motor vehicles and motorcycles')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('55', 'I55', 'Accomodation and Food Service Activities', 'Accommodation')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('60', 'H49', 'Transportation and Storage', 'Land transport and transport via pipelines')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('61', 'H50', 'Transportation and Storage', 'Water transport')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('62', 'H51', 'Transportation and Storage', 'Air transport')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('63', 'H52', 'Transportation and Storage', 'Warehousing and support activities for transportation')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('64', 'H53', 'Postal and courier activities', 'Postal and courier activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('65', 'K64', 'Financial and Insurance Activities', 'Financial service activities, except insurance and pension funding')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('66', 'K65', 'Financial and Insurance Activities', 'Insurance, reinsurance and pension funding, except compulsory social security')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('67', 'K66', 'Financial and Insurance Activities', 'Activities auxiliary to financial services and insurance activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('70', 'L68', 'Real Estate Activities', 'Real estate activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('71', 'M69', 'Professional, Scientific and Technical Activities', 'Legal and accounting activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('72', 'M70', 'Professional, Scientific and Technical Activities', 'Activities of head offices; management consultancy activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('73', 'M71', 'Professional, Scientific and Technical Activities', 'Architectural and engineering activities; technical testing and analysis')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('74', 'N82', 'Administrative and Support Service Activities', 'Office administrative, office support and other business support activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('75', 'O84', 'Public Administration and Defence; Compulsory Social Security', 'Public administration and defence; compulsory social security')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('80', 'P85', 'Education', 'Education')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('85', 'Q86', 'Human Health and Social Work Activities', 'Human health activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('90', 'R90', 'Arts, Entertainment and Recreation', 'Creative, arts and entertainment activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('91', 'R91', 'Arts, Entertainment and Recreation', 'Libraries, archives, museums and other cultural activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('92', 'R92', 'Arts, Entertainment and Recreation', 'Gambling and betting activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('93', 'S96', 'Other Service Activities', 'Other personal service activities')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('95', 'T97', 'Activities of Households as Employers; Undifferentiated Goods- and Services- Producing Activities of Households for Own Use', 'Activities of households as employers of domestic personnel')
INSERT INTO SicNaceCodeMap (SicFirstTwoDigits, NaceCode, Category, Description) VALUES ('99', 'U99', 'Activities of Extraterritorial Organisations and Bodies', 'Activities of extraterritorial organisations and bodies')

END
GO
