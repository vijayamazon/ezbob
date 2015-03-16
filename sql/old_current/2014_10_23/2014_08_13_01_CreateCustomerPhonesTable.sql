IF OBJECT_ID('CustomerPhones') IS NULL
BEGIN
	CREATE TABLE CustomerPhones 
	(
		Id INT IDENTITY NOT NULL,
		CustomerId INT,
		PhoneType NVARCHAR(20),
		Phone NVARCHAR(50),
		IsVerified BIT,
		VerificationDate DATETIME,
		VerifiedBy NVARCHAR(100)
	)
	
	-- Backfill phones
	DECLARE @CustomerId INT, @MobilePhone NVARCHAR(50), @DaytimePhone NVARCHAR(50)

	DECLARE cur CURSOR FOR 
		SELECT 
			Id, 
			MobilePhone 
		FROM
			Customer
		WHERE 
			MobilePhone IS NOT NULL
			
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId, @MobilePhone
	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO CustomerPhones (CustomerId,	PhoneType, Phone, IsVerified, VerificationDate,	VerifiedBy) VALUES (@CustomerId, 'Mobile', @MobilePhone, 0, NULL, NULL)
		
		FETCH NEXT FROM cur INTO @CustomerId, @MobilePhone
	END

	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR 
		SELECT 
			Id, 
			DaytimePhone 
		FROM
			Customer
		WHERE 
			DaytimePhone IS NOT NULL
			
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId, @DaytimePhone
	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO CustomerPhones (CustomerId,	PhoneType, Phone, IsVerified, VerificationDate,	VerifiedBy) VALUES (@CustomerId, 'Daytime', @DaytimePhone, 0, NULL, NULL)
		
		FETCH NEXT FROM cur INTO @CustomerId, @DaytimePhone
	END

	CLOSE cur
	DEALLOCATE cur	
END
GO
