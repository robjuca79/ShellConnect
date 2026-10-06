
@echo off

set build="C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
echo Value of build: %build%


if exist "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" (
  echo File exists!
  CALL :rebuild %build%
  goto: eof
)
else (
  echo File does not exist.
  pause
)

CALL :error "Could not find Visual Studio directory."

:error
    echo rebuild failed: %1
	pause
	goto: eof

:rebuild
    GOTO:rebuild_1

:rebuild_1
echo CLEANUP BIN
cd "D:\GitHub\ShellConnect\Bin"
erase /q /s Connect*.*
cd "D:\SPAD.neXt\Addons"
erase /q /s Connect*.*
echo DONE
echo.
echo.
	echo.
	echo.
	echo  Shell Connect
	echo.
	echo. 
	rem "do not change this order"
	echo -- Provider Enumerator ...
	%build% "D:\GitHub\ShellConnect\Provider\Enumerator\Provider.Enumerator\Connect Provider Enumerator.slnx" /t:rebuild /verbosity:minimal /nologo 
	echo.
	echo.
	echo.
	echo -- Provider Data ...
	%build% "D:\GitHub\ShellConnect\Provider\Data\Provider.Data\Connect Provider Data.slnx" /t:rebuild /verbosity:minimal /nologo 
	echo.
	echo.
	echo.
	echo -- Provider Interface  ...
	%build% "D:\GitHub\ShellConnect\Provider\Interface\Provider.Interface\Connect Provider Interface.slnx" /t:rebuild /verbosity:minimal /nologo 
	echo.
	echo.
	echo.
	echo -- Process Dispatcher  ...
	%build% "D:\GitHub\ShellConnect\Process\Dispatcher\Process.Dispatcher\Connect Process Dispatcher.slnx" /t:rebuild /verbosity:minimal /nologo 
	echo.
	echo.
	echo.
	echo -- Stub Bootstrapper  ...
	%build% "D:\GitHub\ShellConnect\Stub\Bootstrapper\Connect.Bootstrapper\Connect Bootstrapper.slnx" /t:rebuild /verbosity:minimal /nologo 
	echo.
	echo.
	echo.
	echo -- Stub Shell  ...
	%build% "D:\GitHub\ShellConnect\Stub\Shell\Connect.Shell\Connect Shell.slnx" /t:rebuild /verbosity:minimal /nologo 
	echo.
	echo.
	echo.
	echo ALL DONE
	echo.
	echo.
	pause