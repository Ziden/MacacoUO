@SET CURPATH=%~dp0
@SET CSCPATH=%CURPATH%bin\roslyn\

@SET SDKPATH=%CURPATH%Ultima\
@SET SRVPATH=%CURPATH%Server\

@SET EXENAME=MacacoUO

@TITLE: %EXENAME% - MacacoUO


::##########

@ECHO:
@ECHO: Compila %EXENAME% pra Windaum
@ECHO:

@DEL "%CURPATH%%EXENAME%.exe"

@ECHO ON

"%CSCPATH%csc.exe" /win32icon:"%SRVPATH%servuo.ico" /r:"%CURPATH%Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll" /target:exe /out:"%CURPATH%%EXENAME%.exe" /recurse:"%SRVPATH%*.cs" /d:ServUO /d:NEWTIMERS /d:NETFX_472 /d:DEBUG /nowarn:0618 /debug /nologo /unsafe

@ECHO OFF

@ECHO:
@ECHO: Feito
@ECHO:


@ECHO:
@ECHO: Rodando
@ECHO:


@ECHO OFF

"%CURPATH%%EXENAME%.exe" -debug

@ECHO:
@ECHO: Pronto
@ECHO:


@ECHO OFF

"%CURPATH%%EXENAME%.exe"