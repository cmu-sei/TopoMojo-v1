#
#multi-stage target: dev
#
FROM dockreg.cwd.local/dotnet-sdk:2.1.302 AS dev

ENV ASPNETCORE_URLS=http://*:5000 \
    ASPNETCORE_ENVIRONMENT=DEVELOPMENT

COPY . /app
WORKDIR /app/src/TopoMojo.Web
RUN bower install
RUN dotnet publish -c Release -o /app/dist
RUN npm install && ng build --prod --output-path /app/dist/wwwroot
CMD ["dotnet", "run"]

#
#multi-stage target: prod
#
FROM dockreg.cwd.local/dotnet:2.1 AS prod
COPY --from=dev /app/dist /app
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000
CMD [ "dotnet", "TopoMojo.Web.dll" ]
