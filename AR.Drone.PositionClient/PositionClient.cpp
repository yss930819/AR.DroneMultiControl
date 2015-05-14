// 这是主 DLL 文件。

// 通过本文件调用已经写好的类实现数据的传输

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

	int err;
	
	int* pLen = new int();
	*pLen = len;
	

	err=recvfrom(sockSrv,(char *)(_quadrotor),length,0,(SOCKADDR*)addrClient,pLen);

	if (err!=-1)
	{

		//数据格式转换,
		_quadrotor->ConverData();
		//输出飞机经纬度和高度
		//std::cout<<_quadrotor->longitude<<std::endl;
		//std::cout<<_quadrotor->latitude<<std::endl;
		//std::cout<<_quadrotor->altitude<<std::endl;
		//std::cout<<_quadrotor->phi<<std::endl;
		//std::cout<<_quadrotor->theta<<std::endl;
		//std::cout<<_quadrotor->psi<<std::endl;
	}      
}

//************************************
// Method:    initSocket
// FullName:  AR::Drone::Client::PositionClient::initSocket
// Access:    public 
// Returns:   void
// Qualifier: 调用获取数据之前必须调用本函数
//************************************
void AR::Drone::Client::PositionClient::initSocket()
{
	//SOCKET初始化
	position = new diyPosition();
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

	addrClient = new SOCKADDR_IN();
}



diyPosition* AR::Drone::Client::PositionClient::getPosition1()
{
	position->x = _quadrotor->longitude;
	position->y = _quadrotor->latitude;
	position->z = _quadrotor->altitude;
	return position;
}

diyPosition* AR::Drone::Client::PositionClient::getPosition2()
{
	position->x = _quadrotor->agl;
	position->y = _quadrotor->phi;
	position->z = _quadrotor->theta;
	return position;
}






