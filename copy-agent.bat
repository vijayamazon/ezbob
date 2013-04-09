net stop ScortoService
robocopy App\service\agent\ c:\ezbobSrv\ /e
robocopy Integration\NodeEzBobLib\bin\Debug\ c:\ezbobSrv\ /e
robocopy Integration\CustomSchedulers\bin\Debug\ c:\ezbobSrv\ /e
robocopy Integration\EzBob.Signals\bin\Debug\ c:\ezbobSrv\ /e
robocopy Integration\EKM\bin\Debug\ c:\ezbobSrv\ /e
copy Lib\Scorto.Core\DBSQLServer.dll c:\ezbobSrv\ 
net start ScortoService