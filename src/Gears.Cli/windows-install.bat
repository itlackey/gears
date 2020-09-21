dotnet publish -o %1 -c release -r win-x64

#dotnet build -c Release && dotnet tool update itl-gears --add-source ./.nupkg/ -g