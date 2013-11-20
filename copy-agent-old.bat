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

robocopy Integration\FraudChecker\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\ZooplaLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\Amazon\AmazonDbLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\Amazon\AmazonLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\Amazon\AmazonServiceLib\bin\ c:\ezbobSrv\ /e

robocopy Integration\Amazon\FBAInventoryServiceMWS\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\eBay\eBayDbLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\eBay\eBayLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\eBay\eBayServiceLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\ExperianLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\EzBob\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\EzBob.Configuration\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\MailApi\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\EzBob.Models\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\PayPal\PayPal.Base\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\PayPal\PayPal.Permissions\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\PayPal\PayPalServiceLib\bin\Debug\ c:\ezbobSrv\ /e

robocopy Integration\TeraPeak\TeraPeakServiceLib\bin\Debug\ c:\ezbobSrv\ /e

copy Lib\Scorto.Core\DBSQLServer.dll c:\ezbobSrv\ 

net start ScortoService


echo Alles in ordnung.

