DECLARE 
	@CustomerId INT, 
	@FirstName NVARCHAR(250), 
	@MiddleInitial NVARCHAR(250), 
	@Surname NVARCHAR(250)

DECLARE cur CURSOR FOR 
	SELECT 
		Customer.Id, 
		Customer.FirstName,
		Customer.MiddleInitial,
		Customer.Surname 
	FROM 
		Customer

OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId, @FirstName, @MiddleInitial, @Surname
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF @MiddleInitial IS NULL OR @MiddleInitial = ''
			UPDATE Customer SET Fullname = @FirstName + ' ' + @Surname WHERE Id = @CustomerId
		ELSE
			UPDATE Customer SET Fullname = @FirstName + ' ' + @Surname + ' ' + @MiddleInitial WHERE Id = @CustomerId
			
		FETCH NEXT FROM cur INTO @CustomerId, @FirstName, @MiddleInitial, @Surname
	END
	CLOSE cur
	DEALLOCATE cur	
GO
