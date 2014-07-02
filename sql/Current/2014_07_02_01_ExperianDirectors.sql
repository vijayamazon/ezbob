IF OBJECT_ID('ExperianDirectors') IS NULL
BEGIN
	CREATE TABLE ExperianDirectors (
		ExperianDirectorID INT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		AddressID INT NOT NULL,
		FirstName NVARCHAR(512) NOT NULL,
		MiddleName NVARCHAR(512) NOT NULL,
		LastName NVARCHAR(512) NOT NULL,
		BirthDate DATETIME NOT NULL,
		Gender NCHAR(1) NULL,
		Email NVARCHAR(512) NULL,
		MobilePhone NVARCHAR(50) NULL,
		IsDirector BIT NOT NULL,
		IsShareholder BIT NOT NULL,
		IsDeleted BIT NOT NULL,
		CONSTRAINT PK_ExperianDirector PRIMARY KEY (ExperianDirectorID),
		CONSTRAINT FK_ExperianDirector_Customer FOREIGN KEY (CustomerID) REFERENCES Customer (Id),
		CONSTRAINT FK_ExperianDirector_Address FOREIGN KEY (AddressID) REFERENCES CustomerAddress (addressId)
	)
END
GO
