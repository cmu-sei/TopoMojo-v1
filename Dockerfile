#
#multi-stage target: dev
#
FROM dockreg.cwd.local/dotnet-sdk:2.2 AS dev

ENV ASPNETCORE_URLS=http://*:5000 \
    ASPNETCORE_ENVIRONMENT=DEVELOPMENT

COPY . /app
WORKDIR /app/src/topomojo-app
RUN npm install && ./node_modules/@angular/cli/bin/ng build --prod --output-path /app/dist/wwwroot

WORKDIR /app/src/TopoMojo.Web
RUN dotnet publish -c Release -o /app/dist
CMD ["dotnet", "run"]

#
#multi-stage target: prod
#
FROM microsoft/dotnet:2.2-aspnetcore-runtime AS prod
ARG commit
ENV COMMIT=$commit
COPY --from=dev /app/dist /app
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://*:80
CMD [ "dotnet", "TopoMojo.Web.dll" ]
