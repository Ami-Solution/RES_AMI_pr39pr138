@echo off

set dllLocation=C:\Users\Serlok\source\repos\RES_AMI_pr39pr138\AMI_Agregator_Test\bin\Debug\AMI_Agregator_Test.dll
set reportFolderLocation=.

echo * Runing unit test cover *
cd %reportFolderLocation%
call OpenCoverAPI.bat %dllLocation% %reportFolderLocation%\reports * *

pause