﻿DELETE FROM [dbo].[CardInfo]

GO

ALTER TABLE dbo.CardInfo ADD CONSTRAINT
    PK_CardInfo PRIMARY KEY CLUSTERED 
    (
    Id
    ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

DECLARE @card_id INT
SET @card_id=1
DECLARE @custid INT, @AccountNumber VARCHAR (max), @BWAResult VARCHAR(max), @BankAccountType VARCHAR(max), @SortCode VARCHAR(max)

DECLARE tp_Cursor CURSOR FOR 
SELECT id, AccountNumber, BWAResult, BankAccountType, SortCode  FROM  customer where AccountNumber is not null
OPEN tp_Cursor;
FETCH NEXT FROM tp_Cursor INTO @custid, @AccountNumber, @BWAResult, @BankAccountType, @SortCode
WHILE @@FETCH_STATUS = 0
BEGIN
SET @card_id=@card_id+1;
INSERT INTO cardinfo (Id, CustomerId, BankAccount,BWAResult, BankAccountType, SortCode)
VALUES
(@card_id, @custid, @AccountNumber, @BWAResult, @BankAccountType, @SortCode)
FETCH NEXT FROM tp_Cursor INTO @custid, @AccountNumber, @BWAResult, @BankAccountType, @SortCode;
end
CLOSE tp_Cursor
DEALLOCATE tp_Cursor;
