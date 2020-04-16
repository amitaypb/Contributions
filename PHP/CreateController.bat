REM Go to the Folder of the PHP Project
cd "C:\Projects\PHP\Symfony_Apps\Test_Symfony_App"

@echo off
SET /P ControllerName=Type the name of the Controller to be added:

php bin/console make:controller %ControllerName%
pause