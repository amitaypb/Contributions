@echo off
SET /P ProjectName=Type the name of the Project that you want to create with Symfony:
@echo. & @echo.

REM Run this if you are building a traditional web application.
symfony new %ProjectName% --full

@echo The %ProjectName% Project was created successfully!
Pause