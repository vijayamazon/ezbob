CREATE OR REPLACE VIEW SECURITY_VUSER AS
SELECT u.UserId,
       u.UserName,
       u.FullName,
       u.EMail,
       u.IsDeleted,
       s.SessionCreationDate,
       u.CreationDate,
       u.passexpperiod,
       u.forcepasschange,
       u.disablepasschange,
       u.CERTIFICATETHUMBPRINT
  FROM Security_User u
     LEFT OUTER JOIN (
                      SELECT UserId, MAX(CreationDate) AS SessionCreationDate
                        FROM Security_Session
                       GROUP BY UserId
                     ) s ON s.UserId = u.UserId
/