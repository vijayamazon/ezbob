UPDATE customer SET CurrentDebitCard = NULL
WHERE CurrentDebitCard NOT IN
(
SELECT id FROM CardInfo
) 

ALTER TABLE dbo.Customer WITH nocheck ADD CONSTRAINT
 FK_Customer_CardInfo FOREIGN KEY
 (
 CurrentDebitCard
 ) REFERENCES dbo.CardInfo
 (
 Id
 ) ON UPDATE  NO ACTION 
  ON DELETE  NO ACTION