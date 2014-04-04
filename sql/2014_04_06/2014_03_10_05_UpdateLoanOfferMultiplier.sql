IF EXISTS (SELECT 1 FROM LoanOfferMultiplier WHERE StartScore = 0 AND EndScore = 650)
BEGIN
	UPDATE LoanOfferMultiplier SET StartScore = 550, EndScore = 649 WHERE StartScore = 0 AND EndScore = 650
	UPDATE LoanOfferMultiplier SET StartScore = 650 WHERE StartScore = 651
	INSERT INTO LoanOfferMultiplier	(StartScore, EndScore, Multiplier)
		VALUES (0, 549, 0.0)
END
GO

