UPDATE Customer
SET FirstName=UPPER(LEFT(FirstName,1))+SUBSTRING(FirstName,2,LEN(FirstName)),
	Surname=UPPER(LEFT(Surname,1))+SUBSTRING(Surname,2,LEN(Surname)),
	MiddleInitial=UPPER(LEFT(MiddleInitial,1))+SUBSTRING(MiddleInitial,2,LEN(MiddleInitial))
