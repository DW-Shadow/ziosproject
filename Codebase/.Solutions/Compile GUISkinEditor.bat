@ECHO OFF
setlocal ENABLEDELAYEDEXPANSION
set unity=%1
svn info -r HEAD --show-item revision > revision.txt
set /p revision=<revision.txt
if "%unity%"=="" set /p unity=Unity Version? 
set versions=UNITY_EDITOR;UNITY_THEMES;
if %unity% geq 5 if %unity% lss 6 set versions=%versions%UNITY_5;
if %unity% geq 5.0 if %unity% lss 5.1 set versions=%versions%UNITY_5_0;
if %unity% geq 5.1 if %unity% lss 5.2 set versions=%versions%UNITY_5_1;
if %unity% geq 5.2 if %unity% lss 5.3 set versions=%versions%UNITY_5_2;
if %unity% geq 5.3 if %unity% lss 5.4 set versions=%versions%UNITY_5_3;
if %unity% geq 5.4 if %unity% lss 5.5 set versions=%versions%UNITY_5_4;
if %unity% geq 5.5 if %unity% lss 5.6 set versions=%versions%UNITY_5_5;
if %unity% geq 5.6 if %unity% lss 5.7 set versions=%versions%UNITY_5_6;
if %unity% geq 5.3 set versions=%versions%UNITY_5_3_OR_NEWER;
if %unity% geq 5.3.4 set versions=%versions%UNITY_5_3_4_OR_NEWER;
if %unity% geq 5.4 set versions=%versions%UNITY_5_4_OR_NEWER;
if %unity% geq 5.5 set versions=%versions%UNITY_5_5_OR_NEWER;
if %unity% geq 5.6 set versions=%versions%UNITY_5_6_OR_NEWER;
set versions=%versions%UNITY_%unity:.=_%;
set extra=/p:DefineConstants="%versions%"
echo %extra%
copy /Y "%unity%\*" . >NUL
copy /Y "Properties\AssemblyInfoGUISkinEditor.cs" . >NUL
ren AssemblyInfoGUISkinEditor.cs AssemblyInfo.cs
powershell -Command "(gc AssemblyInfo.cs) -replace '{unityversion}', '%unity%' | Out-File AssemblyInfo.cs"
powershell -Command "(gc AssemblyInfo.cs) -replace '{revision}', '%revision%' | Out-File AssemblyInfo.cs"
IF EXIST "%programfiles%\MSBuild\14.0\Bin" GOTO USE_MSBUILD14_X86
IF EXIST "%programfiles(x86)%\MSBuild\14.0\Bin" GOTO USE_MSBUILD14_X64
IF EXIST "%programfiles%\MSBuild\15.0\Bin" GOTO USE_MSBUILD15_X86
IF EXIST "%programfiles(x86)%\MSBuild\15.0\Bin" GOTO USE_MSBUILD15_X64
GOTO CLOSE
:USE_MSBUILD14_X86
	echo Compiling ZiosGUISkinEditor.dll - MSBuild 14.0 (on 32-bit host)...
	"%programfiles%\MSBuild\14.0\Bin\MSBuild.exe" ZiosGUISkinEditor.sln %extra%
	GOTO CLOSE
:USE_MSBUILD14_X64
	echo Compiling ZiosGUISkinEditor.dll - MSBuild 14.0 (on 64-bit host)...
	"%programfiles(x86)%\MSBuild\14.0\Bin\amd64\MSBuild.exe" ZiosGUISkinEditor.sln %extra%
	GOTO CLOSE
:USE_MSBUILD15_X86
	echo Compiling ZiosGUISkinEditor.dll - MSBuild 15.0 (on 32-bit host)...
	"%programfiles%\MSBuild\15.0\Bin\MSBuild.exe" ZiosGUISkinEditor.sln %extra%
	GOTO CLOSE
:USE_MSBUILD15_X64
	echo Compiling ZiosGUISkinEditor.dll - MSBuild 15.0 (on 64-bit host)...
	"%programfiles(x86)%\MSBuild\15.0\Bin\amd64\MSBuild.exe" ZiosGUISkinEditor.sln %extra%
	GOTO CLOSE
:CLOSE
copy /Y "%unity%\*" . >NUL
rmdir /Q /S obj
move /Y ZiosGUISkinEditor.dll Release
del revision.txt >nul 2>&1
del AssemblyInfo.cs
del Unity*.dll >nul 2>&1
cd Release
del ZiosGUISkinEditor-r%revision%-%unity%.dll >nul 2>&1
ren ZiosGUISkinEditor.dll ZiosGUISkinEditor-r%revision%-%unity%.dll
if "%1"=="" pause
