#
#multi-stage target: dev
#
FROM dockreg.cwd.local/dotnet-sdk:2.1.4 AS dev

ENV ASPNETCORE_URLS=http://0.0.0.0:5000 \
    ASPNETCORE_ENVIRONMENT=DEVELOPMENT

COPY . /app

WORKDIR /app/src/TopoMojo.Web

RUN dotnet publish -o /app/dist

CMD ["dotnet", "run"]

#
#multi-stage target: prod
#
FROM dockreg.cwd.local/dotnet:2.0.5 AS prod
WORKDIR /app
COPY --from=dev /app/dist .
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENTRYPOINT [ 'dotnet", '"TopoMojo.Web.dll" ]
