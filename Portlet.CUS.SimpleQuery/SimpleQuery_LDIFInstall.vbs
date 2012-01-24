
set wshShell = Wscript.createobject("wscript.shell")

wshShell.run "%COMSPEC% /k C:\WINDOWS\ADAM\ldifde.exe  -i -f SimpleQuery.ldf -c ""CN=Schema,CN=Configuration,CN=X"" ""#schemaNamingContext"" -k -s localhost -v"
