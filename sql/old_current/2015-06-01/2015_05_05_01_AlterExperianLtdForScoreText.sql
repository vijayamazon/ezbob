IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CreditText1' and Object_ID = Object_ID(N'experianLtd'))
BEGIN
    ALTER TABLE experianLtd ADD CreditText1 nvarchar(560)
END
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CreditText2' and Object_ID = Object_ID(N'experianLtd'))
BEGIN
    ALTER TABLE experianLtd ADD CreditText2 nvarchar(110)
END
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CreditText3' and Object_ID = Object_ID(N'experianLtd'))
BEGIN
    ALTER TABLE experianLtd ADD CreditText3 nvarchar(110)
END
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CreditText4' and Object_ID = Object_ID(N'experianLtd'))
BEGIN
    ALTER TABLE experianLtd ADD CreditText4 nvarchar(110)
END
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CreditText5' and Object_ID = Object_ID(N'experianLtd'))
BEGIN
    ALTER TABLE experianLtd ADD CreditText5 nvarchar(110)
END
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'ConclusionText' and Object_ID = Object_ID(N'experianLtd'))
BEGIN
    ALTER TABLE experianLtd ADD ConclusionText nvarchar(300)
END