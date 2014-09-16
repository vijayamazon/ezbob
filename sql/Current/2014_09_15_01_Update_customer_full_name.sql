UPDATE Customer SET
	Fullname = LTRIM(RTRIM(
		LTRIM(RTRIM(
			LTRIM(RTRIM(ISNULL(FirstName, ''))) + ' ' +
			LTRIM(RTRIM(ISNULL(MiddleInitial, '')))
		)) + ' ' + LTRIM(RTRIM(ISNULL(Surname, '')))
	))
GO
