FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview7-alpine3.9 AS build-env
WORKDIR /app

# 安装git
RUN apk add --no-cache git
RUN git clone https://github.com/adams549659584/MyDotNetCoreApp.git

# 设置项目文件夹为工作区
WORKDIR /app/MyDotNetCoreApp
RUN dotnet restore

# 设置项目job文件夹为工作区
WORKDIR /app/MyDotNetCoreApp/My.App.Job
RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.0.0-preview7-alpine3.9
WORKDIR /app
COPY --from=build-env /app/MyDotNetCoreApp/My.App.Job/out ./
ENTRYPOINT ["dotnet", "My.App.Job.dll"]

# docker build -t dotnetcore-myjob .

# dotnet publish -c Release -o app
# FROM mcr.microsoft.com/dotnet/core/runtime:3.0.0-preview7-alpine3.9
# WORKDIR /app
# COPY /app ./
# ENTRYPOINT ["dotnet", "My.App.Job.dll"]