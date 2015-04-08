// 这是主 DLL 文件。

#include "stdafx.h"
#include "quadrotor.h"
#include "PositionClient.h"
#include <Winsock2.h>
#include <stdio.h>
#include <iostream>


#pragma comment(lib,"ws2_32.lib")


using namespace std;
//c++中命名空间要连续使用
using namespace AR;
using namespace Drone;
using namespace Client;


PositionClient::PositionClient()
{
	this->_quadrotor = new quadrotor();
}

PositionClient::~PositionClient()
{
	closesocket(sockSrv);
	this->_quadrotor->~quadrotor();
}

void PositionClient::RecevieData()
{
	cout<< "运行"<<length<<endl;
	int err;
	
	int* pLen = new int();
	*pLen = len;
	addrClient = new SOCKADDR_IN();

	err=recvfrom(sockSrv,(char *)(_quadrotor),length,0,(SOCKADDR*)addrClient,pLen);
	cout<< len<<"::"<<*pLen<<"::"<<addrClient<<"::"<<sockSrv<<endl;
	cout<<err<<endl;

	if (err!=-1)
	{

		//数据格式转换,
		_quadrotor->ConverData();
		//输出飞机经纬度和高度
		std::cout<<_quadrotor->longitude<<std::endl;
		std::cout<<_quadrotor->latitude<<std::endl;
		std::cout<<_quadrotor->altitude<<std::endl;
		std::cout<<_quadrotor->phi<<std::endl;
		std::cout<<_quadrotor->theta<<std::endl;
		std::cout<<_quadrotor->psi<<std::endl;
	}      
}

void AR::Drone::Client::PositionClient::initSocket()
{
	//SOCKET初始化
	WORD wVersionRequested;
	WSADATA wsaData;
	int err;

	wVersionRequested = MAKEWORD( 1, 1 );

	err = WSAStartup( wVersionRequested, &wsaData );
	if ( err != 0 ) {
		return;
	}


	if ( LOBYTE( wsaData.wVersion ) != 1 ||
		HIBYTE( wsaData.wVersion ) != 1 ) {
			WSACleanup( );
			return; 
	}

	sockSrv=socket(AF_INET,SOCK_DGRAM,0);
	SOCKADDR_IN addrSrv;
	addrSrv.sin_addr.S_un.S_addr=htonl(INADDR_ANY);
	addrSrv.sin_family=AF_INET;

	//端口5500
	addrSrv.sin_port=htons(5500);

	bind(sockSrv,(SOCKADDR*)&addrSrv,sizeof(SOCKADDR));

	
	len = sizeof(SOCKADDR);
	length = sizeof(quadrotor);
}






