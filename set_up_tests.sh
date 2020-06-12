dotnet new sln -o unit-tests
cd unit-tests
dotnet new classlib -o Utilb
mv ./Util/Class1.cs Utilb.cs
dotnet sln add ./Utilb/Utilb.csproj
dotnet new xunit -o Utilb.Tests
dotnet add ./Utilb.Tests/Utilb.Tests.csproj reference ./Utilb/Utilb.csproj
dotnet sln add ./Utilb.Tests/Utilb.Tests.csproj
