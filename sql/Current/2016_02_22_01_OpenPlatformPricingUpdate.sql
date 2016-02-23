DECLARE @EzbobOriginID INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='ezbob' )
DECLARE @EverlineOriginID INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='everline' )
DECLARE @AlibabaOriginID INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name='alibaba' )

DECLARE @StandardSourceID INT = (SELECT LoanSourceID FROM LoanSource WHERE LoanSourceName='Standard')
DECLARE @CosmeSourceID INT = (SELECT LoanSourceID FROM LoanSource WHERE LoanSourceName='COSME')

DECLARE @GradeA INT = (SELECT GradeID FROM I_Grade WHERE Name='A')
DECLARE @GradeB INT = (SELECT GradeID FROM I_Grade WHERE Name='B')
DECLARE @GradeC INT = (SELECT GradeID FROM I_Grade WHERE Name='C')
DECLARE @GradeD INT = (SELECT GradeID FROM I_Grade WHERE Name='D')
DECLARE @GradeE INT = (SELECT GradeID FROM I_Grade WHERE Name='E')
DECLARE @GradeF INT = (SELECT GradeID FROM I_Grade WHERE Name='F')
DECLARE @GradeG INT = (SELECT GradeID FROM I_Grade WHERE Name='G')
DECLARE @GradeH INT = (SELECT GradeID FROM I_Grade WHERE Name='H')

DECLARE @SubGradeA1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='A1')
DECLARE @SubGradeA2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='A2')
DECLARE @SubGradeA3 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='A3')
DECLARE @SubGradeA4 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='A4')

DECLARE @SubGradeB1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='B1')
DECLARE @SubGradeB2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='B2')

DECLARE @SubGradeC1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='C1')

DECLARE @SubGradeD1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='D1')
DECLARE @SubGradeD2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='D2')

DECLARE @SubGradeE1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='E1')
DECLARE @SubGradeE2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='E2')

DECLARE @SubGradeF1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='F1')

DECLARE @SubGradeG1 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='G1')
DECLARE @SubGradeG2 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='G2')
DECLARE @SubGradeG3 INT = (SELECT SubGradeID FROM I_SubGrade WHERE Name='G3')

--min ineterest rate for new customers
UPDATE I_GradeRange SET MinInterestRate = 0.0165 WHERE IsFirstLoan=1 AND SubGradeID IN (@SubGradeC1,@SubGradeD1,@SubGradeD2)
UPDATE I_GradeRange SET MinInterestRate = 0.0205 WHERE IsFirstLoan=1 AND SubGradeID IN (@SubGradeE1,@SubGradeE2)
UPDATE I_GradeRange SET MinInterestRate = 0.0275 WHERE IsFirstLoan=1 AND SubGradeID IN (@SubGradeF1)
UPDATE I_GradeRange SET MinInterestRate = 0.0325 WHERE IsFirstLoan=1 AND SubGradeID IN (@SubGradeG1,@SubGradeG2,@SubGradeG3)

--min ineterest rate for not new customers
UPDATE I_GradeRange SET MinInterestRate = 0.0160 WHERE IsFirstLoan=0 AND SubGradeID IN (@SubGradeC1,@SubGradeD1,@SubGradeD2)
UPDATE I_GradeRange SET MinInterestRate = 0.0195 WHERE IsFirstLoan=0 AND SubGradeID IN (@SubGradeE1,@SubGradeE2)
UPDATE I_GradeRange SET MinInterestRate = 0.0275 WHERE IsFirstLoan=0 AND SubGradeID IN (@SubGradeF1)
UPDATE I_GradeRange SET MinInterestRate = 0.0325 WHERE IsFirstLoan=0 AND SubGradeID IN (@SubGradeG1,@SubGradeG2,@SubGradeG3)


--max ineterest rate
UPDATE I_GradeRange SET MaxInterestRate = 0.0225 WHERE SubGradeID IN (@SubGradeA1,@SubGradeA2,@SubGradeA3,@SubGradeA4,@SubGradeB1,@SubGradeB2,@SubGradeC1)
UPDATE I_GradeRange SET MaxInterestRate = 0.0250 WHERE SubGradeID IN (@SubGradeD2)

--min setupfee 
UPDATE I_GradeRange SET MinSetupFee = 0.0050 WHERE SubGradeID IN (@SubGradeA1,@SubGradeA2,@SubGradeA3,@SubGradeA4,@SubGradeB1,@SubGradeB2,@SubGradeC1)
UPDATE I_GradeRange SET MinSetupFee = 0.0100 WHERE SubGradeID IN (@SubGradeD1,@SubGradeD2,@SubGradeE1,@SubGradeE2)
UPDATE I_GradeRange SET MinSetupFee = 0.0200 WHERE SubGradeID IN (@SubGradeF1,@SubGradeG1,@SubGradeG2,@SubGradeG3)
UPDATE I_GradeRange SET MinSetupFee = 0.0050 WHERE GradeID IS NULL


--max setupfee
UPDATE I_GradeRange SET MaxSetupFee = 0.0700 WHERE SubGradeID IN (@SubGradeA1,@SubGradeA2,@SubGradeA3,@SubGradeA4,@SubGradeB1,@SubGradeB2,@SubGradeC1,@SubGradeC1,@SubGradeD1,@SubGradeD2,@SubGradeE1,@SubGradeE2)
UPDATE I_GradeRange SET MaxSetupFee = 0.0750 WHERE OriginID=@EzbobOriginID AND SubGradeID IN (@SubGradeF1)
UPDATE I_GradeRange SET MaxSetupFee = 0.0800 WHERE SubGradeID IN (@SubGradeG1,@SubGradeG2,@SubGradeG3)

UPDATE I_GradeRange SET MaxSetupFee = 0.0700 WHERE OriginID=@EverlineOriginID
GO


