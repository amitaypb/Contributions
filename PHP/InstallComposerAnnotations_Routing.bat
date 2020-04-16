@echo off
REM Install the Annotations Package.
REM Instead of defining your route in YAML, Symfony also allows you to use annotation routes.
REM You can now add your route directly above the controller:
REM + use Symfony\Component\Routing\Annotation\Route;
REM class LuckyController
REM {
REM +     /**
REM +      * @Route("/lucky/number")
REM +      */
REM    public function number()
REM    {
REM		// this looks exactly the same
REM	   }
REM }
cd "C:\php"
REM With Symfony Flex
REM composer require annotations

REM Without Symfony Flex
composer require sensio/framework-extra-bundle
pause