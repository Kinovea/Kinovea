# 基础运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

# 构建镜像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 只复制核心项目
COPY ["Kinovea.CameraService/Kinovea.CameraService.csproj", "./Kinovea.CameraService/"]
COPY ["Kinovea.Camera/Kinovea.Camera.csproj", "./Kinovea.Camera/"]
COPY ["Kinovea.Camera.HTTP/Kinovea.Camera.HTTP.csproj", "./Kinovea.Camera.HTTP/"]

# 还原依赖项
WORKDIR "/src/Kinovea.CameraService"
RUN dotnet restore "Kinovea.CameraService.csproj"

# 复制源代码
WORKDIR /src
COPY . .

# 构建项目
WORKDIR "/src/Kinovea.CameraService"
RUN dotnet build "Kinovea.CameraService.csproj" -c Release -o /app/build

# 发布
FROM build AS publish
RUN dotnet publish "Kinovea.CameraService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 最终镜像
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5002
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Kinovea.CameraService.dll"]