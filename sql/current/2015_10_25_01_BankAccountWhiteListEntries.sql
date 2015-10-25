IF NOT EXISTS (SELECT * FROM BankAccountWhiteList WHERE Sortcode='000000' AND BankAccountNumber='00000000')
BEGIN
	INSERT INTO BankAccountWhiteList (Sortcode,BankAccountNumber) VALUES ('000000','00000000')
END
GO

IF NOT EXISTS (SELECT * FROM BankAccountWhiteList WHERE Sortcode='621000' AND BankAccountNumber='20115636')
BEGIN
	INSERT INTO BankAccountWhiteList (Sortcode,BankAccountNumber) VALUES ('621000', '20115636')
END
GO