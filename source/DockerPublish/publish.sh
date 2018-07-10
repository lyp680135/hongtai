#!/bin/bash
# author:小羽科技
# url:www.xiaoyutt.com

# 发布类别
rtype=$1

# 要发布至的服务器IP(这里只要输入生产环境的，测试默认是本机)
serverip=$2

# 要连接至服务器的密钥文件
pemfile=$3

# 镜像名称前缀
name=$4

#端口前缀（与nginx匹配对应）
port=$5

#版本号
ver=$6

# 要发布的项目，以逗号分割
project=`echo $7 | sed -r 's/"//g'`

if [ $rtype == "dev" ]
then
	echo "进入开发发布模式..."

elif [ $rtype == "test" ]
then
	echo "进入测试发布模式...发布项目"$project

	array=(${project//,/ })  
	for var in ${array[@]}
	do
	   if [ $var == "pc" ]
	   then
	   	echo 开始编译、启动"$name"_"$rtype"_pc
	   	# 配置对应的配置文件
		cp -r -f config.test.json ../WebApp/WebSite/config.json
		docker build --force-rm=true --rm -f Dockerfile_pc -t "$name"_"$rtype"_pc ../
		docker ps -a | grep -w "$name"_"$rtype"_pc | awk '{print $1 }'|xargs docker rm -f
		docker run -p "$port"1:5001 -e "ASPNETCORE_ENVIRONMENT=Test"  -v /home/volume/site/"$name"_"$rtype"_pc/UploadFile:/app/UploadFile -v /home/volume/site/"$name"_"$rtype"_pc/logfile:/app/logfile --link mysql:connection --privileged=true --restart=always --name "$name"_"$rtype"_pc -d "$name"_"$rtype"_pc
	   fi

		if [ $var == "wap" ]
		then
			echo 开始编译、启动"$name"_"$rtype"_wap
			cp -r -f config.test.json ../WebApp/WapSite/config.json
			docker build --force-rm=true --rm -f Dockerfile_wap -t "$name"_"$rtype"_wap ../
			docker ps -a | grep -w "$name"_"$rtype"_wap | awk '{print $1 }'|xargs docker rm -f
			docker run -p "$port"2:5002 -e "ASPNETCORE_ENVIRONMENT=Test" -v /home/volume/site/"$name"_"$rtype"_wap/logfile:/app/logfile --link mysql:connection --privileged=true --restart=always --name "$name"_"$rtype"_wap -d "$name"_"$rtype"_wap
		fi

		if [ $var == "webapi" ]
		then
			echo 开始编译、启动"$name"_"$rtype"_webapi
			cp -r -f config.test.json ../WebApp/ApiCenter/config.json
			docker build --force-rm=true --rm -f Dockerfile_webapi -t "$name"_"$rtype"_webapi ../
			docker ps -a | grep -w "$name"_"$rtype"_webapi | awk '{print $1 }'|xargs docker rm -f
			docker run -p "$port"3:5003 -e "ASPNETCORE_ENVIRONMENT=Test" -v /home/volume/site/"$name"_"$rtype"_webapi/logfile:/app/logfile -v /home/volume/site/"$name"_"$rtype"_webapi/qualitypics:/app/qualitypics --link mysql:connection --privileged=true --restart=always --name "$name"_"$rtype"_webapi -d "$name"_"$rtype"_webapi
		fi

		if [ $var == "wapmanage" ]
		then
			echo 开始编译、启动"$name"_"$rtype"_wapmanage
			cp -r -f systemConfig.test.js ../NodeApp/wapmanage/src/plugins/systemConfig.js
			docker build --force-rm=true --rm -f Dockerfile_wapmanage -t "$name"_"$rtype"_wapmanage ../
			docker ps -a | grep -w "$name"_"$rtype"_wapmanage | awk '{print $1 }'|xargs docker rm -f
			docker run -p "$port"4:5004 -e "ASPNETCORE_ENVIRONMENT=Test" --restart=always --name "$name"_"$rtype"_wapmanage -d "$name"_"$rtype"_wapmanage
		fi

	done 
	echo 删除镜像名称为none的镜像
	docker images|grep -wn none|awk '{print $3 }'|xargs docker rmi

elif [ $rtype == "production" ]
then
	echo "进入production发布模式...发布项目"$project

	# 配置对应的配置文件
	cp -r -f config.production.json ../WebApp/WebSite/config.json
	cp -r -f config.production.json ../WebApp/WapSite/config.json
	cp -r -f config.production.json ../WebApp/ApiCenter/config.json
	cp -r -f systemConfig.production.js ../NodeApp/wapmanage/src/plugins/systemConfig.js
	
	arrayproduction=(${project//,/ })  
	for varproduction in ${arrayproduction[@]}
	do
	   	echo 开始编译、启动"$name"_"$rtype"_"$varproduction"

		docker build --force-rm=true --rm -f Dockerfile_"$varproduction" -t registry.cn-hangzhou.aliyuncs.com/xiaoyukeji/"$name"_"$rtype"_"$varproduction":"$ver" ../
		docker push registry.cn-hangzhou.aliyuncs.com/xiaoyukeji/"$name"_"$rtype"_"$varproduction":"$ver"

		echo 删除线上环境 "$name"_"$rtype"_"$varproduction" 容器与镜像
		ssh -i "$pemfile" root@"$serverip" docker rm -f "$name"_"$rtype"_"$varproduction"
		ssh -i "$pemfile" root@"$serverip" docker rmi --force $(docker images | grep "$name"_"$rtype"_"$varproduction" | awk '{print $3}')

		if [ $varproduction == "pc" ]
		then
			ssh -i "$pemfile" root@"$serverip" docker run -p "$port"1:5001  -v /data/volume/site/"$name"_"$rtype"_"$varproduction"/UploadFile:/app/UploadFile -v /data/volume/site/"$name"_"$rtype"_"$varproduction"/logfile:/app/logfile --link mysql:connection --privileged=true --restart=always --name "$name"_"$rtype"_"$varproduction" -d registry.cn-hangzhou.aliyuncs.com/xiaoyukeji/"$name"_"$rtype"_"$varproduction":"$ver"
		fi

		if [ $varproduction == "wap" ]
		then
			ssh -i "$pemfile" root@"$serverip" docker run -p "$port"2:5002 -v /data/volume/site/"$name"_"$rtype"_"$varproduction"/logfile:/app/logfile --link mysql:connection --privileged=true --restart=always --name "$name"_"$rtype"_"$varproduction" -d registry.cn-hangzhou.aliyuncs.com/xiaoyukeji/"$name"_"$rtype"_"$varproduction":"$ver"
		fi

		if [ $varproduction == "webapi" ]
		then
			ssh -i "$pemfile" root@"$serverip" docker run -p "$port"3:5003 -v /data/volume/site/"$name"_"$rtype"_"$varproduction"/logfile:/app/logfile -v /data/volume/site/"$name"_"$rtype"_"$varproduction"/qualitypics:/app/qualitypics --link mysql:connection --privileged=true --restart=always --name "$name"_"$rtype"_"$varproduction" -d registry.cn-hangzhou.aliyuncs.com/xiaoyukeji/"$name"_"$rtype"_"$varproduction":"$ver"
		fi

		if [ $varproduction == "wapmanage" ]
		then
			ssh -i "$pemfile" root@"$serverip" docker run -p "$port"4:5004 --restart=always --name "$name"_"$rtype"_"$varproduction" -d registry.cn-hangzhou.aliyuncs.com/xiaoyukeji/"$name"_"$rtype"_"$varproduction":"$ver"
		fi
		
		echo 删除本地镜像，节省空间
		docker rmi --force $(docker images | grep "$name"_"$rtype"_"$varproduction" | awk '{print $3}')
	done

else
	echo "Error：必须传入参数，且只能传入dev（开发环境）、 test（测试环境）和production（生产环境）"
	exit
fi





