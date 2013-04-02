create table LoanCharges(
	Id int NOT NULL IDENTITY (1, 1),
	Amount DECIMAL,
	LoanId INT,
	ConfigurationVariableId INT
)