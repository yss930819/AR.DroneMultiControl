// ������ DLL �ļ���

// ͨ�����ļ������Ѿ�д�õ���ʵ�����ݵĴ���

#include "stdafx.h"
#include "quadrotor.h"
#include "PositionClient.h"
#include <Winsock2.h>
#include <stdio.h>
#include <iostream>


#pragma comment(lib,"ws2_32.lib")


using namespace std;
//c++�������ռ�Ҫ����ʹ��
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

		//���ݸ�ʽת��,
		_quadrotor->ConverData();
		//����ɻ���γ�Ⱥ͸߶�
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
// Qualifier: ���û�ȡ����֮ǰ������ñ�����
//************************************
void AR::Drone::Client::PositionClient::initSocket()
{
	//SOCKET��ʼ��
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

	//�˿�5500
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






