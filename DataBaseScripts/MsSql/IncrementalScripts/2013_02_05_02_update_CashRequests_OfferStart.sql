DECLARE @custid INT, @crdate DATETIME, @id int, @ApplyForLoan datetime
DECLARE tp_Cursor CURSOR FOR 
	SELECT a.IdCustomer, a.CreationDate  FROM 
	(
	SELECT cr.IdCustomer, max(cr.CreationDate) 'CreationDate'from CashRequests cr
	GROUP BY cr.IdCustomer
	) a	
OPEN tp_Cursor;
FETCH NEXT FROM tp_Cursor INTO @custid, @crdate
WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @id=id, @ApplyForLoan=ApplyForLoan FROM Customer
	WHERE Id=@custid
		
	IF ( 
		SELECT top 1 (OfferStart) FROM CashRequests Id 
		WHERE IdCustomer=@custid AND CreationDate=@crdate
	) IS NULL
		UPDATE CashRequests SET OfferStart = @ApplyForLoan
		WHERE IdCustomer=@id AND CreationDate=@crdate  	
	
FETCH NEXT FROM tp_Cursor INTO @custid, @crdate
end
CLOSE tp_Cursor
DEALLOCATE tp_Cursor;