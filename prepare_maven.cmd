cls
@echo off

set mavenPath=App\clients\Maven\

set p=Integration\EzBob.Models\bin\Debug\EzBob.Models.dll
set p=%p%, Integration\Amazon\AmazonLib\bin\Debug\AmazonLib.dll
set p=%p%, Integration\Amazon\AmazonDbLib\bin\Debug\AmazonDbLib.dll
set p=%p%, Integration\Amazon\AmazonServiceLib\bin\Debug\AmazonServiceLib.dll
set p=%p%, Integration\DatabaseLib\bin\Debug\DatabaseLib.dll
set p=%p%, Integration\eBay\eBayServiceLib\bin\Debug\eBayServiceLib.dll
set p=%p%, Integration\ExperianLib\bin\Debug\ExperianLib.dll
set p=%p%, Integration\EzBob.Configuration\bin\Debug\EzBob.Configuration.dll
rem set p=%p%, Integration\EzBob\bin\Debug\EzBob.dll
set p=%p%, Integration\GuiNodeEzBobLib\bin\Debug\GuiNodeEzBobLib.dll
set p=%p%, Integration\NodeEzBobLib\bin\Debug\NodeEzBobLib.dll
set p=%p%, Integration\PayPal\PayPal.Base\bin\Debug\PayPal.Base.dll
set p=%p%, Integration\PayPal\PayPal.Permissions\bin\Debug\PayPal.Permissions.dll
set p=%p%, Integration\TeraPeak\TeraPeakServiceLib\bin\Debug\TeraPeakServiceLib.dll
set p=%p%, Integration\CommonLib\bin\Debug\CommonLib.dll
set p=%p%, Integration\eBay\eBayLib\bin\Debug\eBayLib.dll
set p=%p%, Integration\eBay\eBayDbLib\bin\Debug\eBayDbLib.dll
set p=%p%, Integration\PaymentServices\bin\Debug\PaymentServices.dll
set p=%p%, Integration\PayPal\PayPalLib\bin\Debug\PayPalLib.dll
set p=%p%, Integration\PayPal\PayPalServiceLib\bin\Debug\PayPalServiceLib.dll
set p=%p%, Lib\Scorto.Core\DefaultSigningProvider.dll
set p=%p%, Lib\Scorto.Core\Scorto.Maven.Mixed.dll
set p=%p%, Lib\Scorto.Core\Scorto.Maven.Application.dll
set p=%p%, Integration\DatabaseLib\bin\Debug\DatabaseLib.dll

@echo:

for %%f in ( %p%) do xcopy /s/y %%f "%mavenPath%" 

@echo:

cd %mavenPath%
Maven.exe
cd ..\..\..