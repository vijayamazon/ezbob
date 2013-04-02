CREATE OR REPLACE PROCEDURE UpdateIsExternal
(
   pSignalId IN NUMBER,
   pIsExternal IN NUMBER
)
AS
BEGIN
	UPDATE Signal
	SET    IsExternal = pIsExternal
	WHERE  id = pSignalId;
END UpdateIsExternal;
/