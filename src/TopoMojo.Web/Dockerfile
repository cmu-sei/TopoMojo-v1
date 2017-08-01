FROM microsoft/aspnetcore-build:1.1.2

COPY . /app

WORKDIR /app

RUN dotnet restore && \
    dotnet build && \
    dotnet msbuild TopoMojo.Web.csproj /t:WebpackDev

EXPOSE 5004/tcp

CMD ["dotnet", "run", "--urls", "http://*:5004", "--environment", "development"]

