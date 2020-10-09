# 常用命令
```sh
# 新建
dotnet new -h

# 生成包
dotnet pack -o ../0_MyPackages/Test -c release

# 引用包
dotnet add packgae MyHelper -v 1.0.0

# 移除包
dotnet remove package MyHelper
```