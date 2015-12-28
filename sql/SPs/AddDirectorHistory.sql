
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AddHistoryDirector') IS NULL
	EXECUTE('CREATE PROCEDURE AddHistoryDirector AS SELECT 1')
GO


ALTER PROCEDURE AddHistoryDirector
@DirectorID INT, 
@CustomerID INT, 
@Name NVARCHAR (512), 
@DateOfBirth DATETIME, 
@Middle     NVARCHAR (512),
@Surname   NVARCHAR (512), 
@Gender NVARCHAR (1),
@Email    NVARCHAR (128),
@Phone NVARCHAR (50),
@CompanyId INT,
@IsShareholder BIT,
@IsDirector BIT,
@UserId INT
AS
BEGIN
	INSERT INTO DirectorHistory (DirectorID, CustomerID, Name, DateOfBirth, Middle,Surname,Gender,Email,Phone,CompanyId,IsShareholder,IsDirector,UserId) VALUES
	(@DirectorID, @CustomerID, @Name, @DateOfBirth, @Middle,@Surname,@Gender,@Email,@Phone,@CompanyId,@IsShareholder,@IsDirector,@UserId)
	
	
END
GO


