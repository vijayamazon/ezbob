@echo off

net stop ScortoService

robocopy App\service\agent\ c:\ezbobSrv\ /e

robocopy Integration\NodeEzBobLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\CustomSchedulers\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\EzBob.Signals\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\EKM\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\ChannelGrabberAPI\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\ChannelGrabberConfig\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\ChannelGrabberFrontend\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\PayPoint\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\PayPal\PayPalLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\YodleeLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy App\service\Servicestarter\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\PaymentServices\bin\Debug\ c:\ezbobSrv\ /e

robocopy App\ScheduledServices\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\Ezbob.MailNode.Runtime\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\Sage\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\FreeAgent\bin\Debug\ c:\ezbobSrv\ /e

robocopy App\Utils\AgentRegistry\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\ZohoCRM\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\Deveel\Deveel.ZohoCRM\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\FraudChecker\bin\Debug\ c:\ezbobSrv\ /e

copy Lib\Scorto.Core\DBSQLServer.dll c:\ezbobSrv\ 

net start ScortoService


echo Alles in ordnung.

