to run assignment 4 
without sql: 
dotnet publish -o .\publish -c Release -p:PublishSingleFile=true 

with sql:
dotnet publish -o .\publish -c Release -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

sc create "AAAAAA" BinPath="Path\To\publish\Assignment4_WindowsService.exe"
