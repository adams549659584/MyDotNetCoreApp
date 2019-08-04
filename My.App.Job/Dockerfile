ARG SdkImageName
ARG RunTimeImageName

FROM ${SdkImageName} AS build-env
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
FROM ${RunTimeImageName}
WORKDIR /app
COPY --from=build-env /app/MyDotNetCoreApp/My.App.Job/out ./
VOLUME /app/Logs /app/Logs
# 安装bash，设置时区为上海
RUN apk add --no-cache bash && apk add tzdata && cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime \
    && echo "Asia/Shanghai" > /etc/timezone \
    && apk del tzdata
ENTRYPOINT ["dotnet", "My.App.Job.dll"]