CREATE OR REPLACE PROCEDURE resetsignal
(
   psignalid IN NUMBER
)
AS
BEGIN
   UPDATE signal
      SET status = 0
    WHERE ID = psignalid;
END resetsignal;
/