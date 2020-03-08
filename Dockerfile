#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM 3.0.103-nanoserver-1909 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM 3.0.103-nanoserver-1909 AS build
WORKDIR /src
COPY ["TaskManager.csproj", ""]
RUN dotnet restore "./TaskManager.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TaskManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManager.csproj" -c Release -o /app/publish
CMD ASPNETCORE_URLS=http://*:$PORT dotnet out/TaskManager.dll

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.dll"]