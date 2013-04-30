IF OBJECT_ID ('dbo.ExperianAccountTypes') IS NOT NULL
BEGIN
    RETURN
END
CREATE TABLE dbo.ExperianAccountTypes
(
	Id VARCHAR(3) NOT NULL,
	Name VARCHAR(100),
	CONSTRAINT PK_ExperianAccountTypes PRIMARY KEY (Id)
)

INSERT INTO ExperianAccountTypes VALUES ('00', 'Bank')
INSERT INTO ExperianAccountTypes VALUES ('01', 'Hire purchase/Conditional sale')
INSERT INTO ExperianAccountTypes VALUES ('02', 'Unsecured loan (personal loans etc)')
INSERT INTO ExperianAccountTypes VALUES ('03', 'Mortgage')
INSERT INTO ExperianAccountTypes VALUES ('04', 'Budget (revolving account)')
INSERT INTO ExperianAccountTypes VALUES ('05', 'Credit card / Store card')
INSERT INTO ExperianAccountTypes VALUES ('06', 'Charge card')
INSERT INTO ExperianAccountTypes VALUES ('07', 'Rental (TV, brown and white goods)')
INSERT INTO ExperianAccountTypes VALUES ('08', 'Mail Order')
INSERT INTO ExperianAccountTypes VALUES ('12', 'CML member')
INSERT INTO ExperianAccountTypes VALUES ('13', 'CML member')
INSERT INTO ExperianAccountTypes VALUES ('14', 'CML member')
INSERT INTO ExperianAccountTypes VALUES ('15', 'Current accounts')
INSERT INTO ExperianAccountTypes VALUES ('16', 'Second mortgage (secured loan)')
INSERT INTO ExperianAccountTypes VALUES ('17', 'Credit sale fixed term')
INSERT INTO ExperianAccountTypes VALUES ('18', 'Communications')
INSERT INTO ExperianAccountTypes VALUES ('19', 'Fixed term deferred payment')
INSERT INTO ExperianAccountTypes VALUES ('20', 'Variable subscription')
INSERT INTO ExperianAccountTypes VALUES ('21', 'Utility')
INSERT INTO ExperianAccountTypes VALUES ('22', 'Finance lease')
INSERT INTO ExperianAccountTypes VALUES ('23', 'Operating lease')
INSERT INTO ExperianAccountTypes VALUES ('24', 'Unpresentable cheques')
INSERT INTO ExperianAccountTypes VALUES ('25', 'Flexible Mortgages')
INSERT INTO ExperianAccountTypes VALUES ('26', 'Consolidated Debt')
INSERT INTO ExperianAccountTypes VALUES ('27', 'Combined Credit Account')
INSERT INTO ExperianAccountTypes VALUES ('28', 'Pay Day Loans')
INSERT INTO ExperianAccountTypes VALUES ('29', 'Balloon HP')
INSERT INTO ExperianAccountTypes VALUES ('30', 'Residential Mortgage')
INSERT INTO ExperianAccountTypes VALUES ('31', 'Buy To Let Mortgage')
INSERT INTO ExperianAccountTypes VALUES ('32', '100+% LTV Mortgage')
INSERT INTO ExperianAccountTypes VALUES ('33', 'Current Account Off Set Mortgage')
INSERT INTO ExperianAccountTypes VALUES ('34', 'Investment Off Set Mortgage')
INSERT INTO ExperianAccountTypes VALUES ('35', 'Shared Ownership Mortgage')
INSERT INTO ExperianAccountTypes VALUES ('36', 'Contingent Liability')
INSERT INTO ExperianAccountTypes VALUES ('37', 'Store Card')
INSERT INTO ExperianAccountTypes VALUES ('38', 'Multi Function Card')
INSERT INTO ExperianAccountTypes VALUES ('39', 'Water')
INSERT INTO ExperianAccountTypes VALUES ('40', 'Gas')
INSERT INTO ExperianAccountTypes VALUES ('41', 'Electricity')
INSERT INTO ExperianAccountTypes VALUES ('42', 'Oil')
INSERT INTO ExperianAccountTypes VALUES ('43', 'Duel Fuel')
INSERT INTO ExperianAccountTypes VALUES ('44', 'Fuel Card (not Motor fuel)')
INSERT INTO ExperianAccountTypes VALUES ('45', 'House Insurance')
INSERT INTO ExperianAccountTypes VALUES ('46', 'Car Insurance')
INSERT INTO ExperianAccountTypes VALUES ('47', 'Life Insurance')
INSERT INTO ExperianAccountTypes VALUES ('48', 'Health Insurance')
INSERT INTO ExperianAccountTypes VALUES ('49', 'Card Protection')
INSERT INTO ExperianAccountTypes VALUES ('50', 'Mortgage Protection')
INSERT INTO ExperianAccountTypes VALUES ('51', 'Payment Protection')
INSERT INTO ExperianAccountTypes VALUES ('52', 'Not Available in CAIS')
INSERT INTO ExperianAccountTypes VALUES ('53', 'Mobile')
INSERT INTO ExperianAccountTypes VALUES ('54', 'Fixed Line')
INSERT INTO ExperianAccountTypes VALUES ('55', 'Cable')
INSERT INTO ExperianAccountTypes VALUES ('56', 'Satellite')
INSERT INTO ExperianAccountTypes VALUES ('57', 'Business Line')
INSERT INTO ExperianAccountTypes VALUES ('58', 'Broadband')
INSERT INTO ExperianAccountTypes VALUES ('59', 'Multi Communications')
INSERT INTO ExperianAccountTypes VALUES ('60', 'Student Loan')
INSERT INTO ExperianAccountTypes VALUES ('61', 'Home Credit')
INSERT INTO ExperianAccountTypes VALUES ('62', 'Education')
INSERT INTO ExperianAccountTypes VALUES ('63', 'Property Rental')
INSERT INTO ExperianAccountTypes VALUES ('64', 'Other Rental')
INSERT INTO ExperianAccountTypes VALUES ('65', 'Not Available in CAIS')
INSERT INTO ExperianAccountTypes VALUES ('66', 'Not Available in CAIS')
INSERT INTO ExperianAccountTypes VALUES ('67', 'Not Available in CAIS')
INSERT INTO ExperianAccountTypes VALUES ('68', 'Not Available in CAIS')
INSERT INTO ExperianAccountTypes VALUES ('69', 'Mortgage and Unsecured Loan')
INSERT INTO ExperianAccountTypes VALUES ('70', 'Gambling')

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


