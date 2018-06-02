@echo off

if "%1" == ""	goto USAGE1   
set library="%1"

if "%2" == ""	goto USAGE2
set out="%2"

set assembly="%3"
if "%3" == ""	set assembly="*"

set type="%4"
if "%4" == ""	set type="*"

set NUNIT_CONSOLE_PATH=C:\Users\Serlok\source\repos\RES_AMI_pr39pr138\NUnit-2.6.4\NUnit-2.6.4\bin\nunit-console.exe
set OPEN_COVER_RUNNER=C:\Users\Serlok\source\repos\RES_AMI_pr39pr138\opencover.4.6.519\OpenCover.Console.exe
set REPORT_GENERATOR=C:\Users\Serlok\source\repos\RES_AMI_pr39pr138\ReportGenerator_2.5.1.0\ReportGenerator.exe
set report_dir=AMISystem_report

mkdir %out%\%report_dir%
%OPEN_COVER_RUNNER% -register:user -target:%NUNIT_CONSOLE_PATH% -targetargs:"/noshadow %library% " -filter:+[%assembly%]%type% -output:"%out%\%report_dir%\report.xml"

%REPORT_GENERATOR% "-reports:%out%\%report_dir%\report.xml" "-targetdir:%out%\%report_dir%\HTML" -reporttypes:Html;HtmlSummary

goto END

:USAGE1
echo "Parammeter #1 library path not set"
goto HELP

:USAGE2
echo "Parammeter #2 report path not set"

goto HELP

:HELP
echo 	Example: My_OpenCover_ReportGenerator.bat F:\NB\3.4.0\CheckoutUnitTesting\UI\Test\UnitTest\WorkManagementTestSuite\bin\x64\Release\TelventDMS.UI.UnitTest.WorkManagementTestSuite.dll F:\NB\3.4.0\CheckoutUnitTesting\UI\Test\UnitTest\WorkManagementTestSuite\report TelventDMS.UI.Components.WorkManagement* *

:END

