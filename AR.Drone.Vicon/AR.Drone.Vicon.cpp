// ������ DLL �ļ���
#include <iostream>
#include <cassert>
#include <string>
#include <vector>
#include <algorithm>	
#include <functional>
#include <limits>
#include<stdio.h>

#include <math.h>

#include <winsock2.h>

#include "ClientCodes.h"


#include "stdafx.h"

#include "AR.Drone.Vicon.h"


#pragma comment(lib,"ws2_32.lib")
using namespace std;


void AR::Drone::Vicon::ViconClient::init()
{
	std::cout << "Example Client" << std::endl;
	//- Initialisation - - - - - ��ʼ�� - - - - - - - - - - - -
	//  Windows-specific initialisation.

	WORD wVersionRequested;//����wVersionRequested����ָ��׼�����ص�Winsock��İ汾��ͨ�������Ǹ��ֽ�ָ������Ҫ��Winsock��ĸ��汾�����ֽ�ָ�����汾
	WSADATA wsaData;
	wVersionRequested = MAKEWORD( 2, 0 ); //�ú� MAKEWORD(X,Y)��ָ��wVersionRequested����ȷֵ(X�Ǹ�λ�ֽڣ�Y�ǵ�λ�ֽ�)
	if(WSAStartup( wVersionRequested, &wsaData ) != 0)    //WSAStartup������������Winsock DLL���г�ʼ����Э��Winsock �İ汾֧�֣��������Ҫ����Դ
	{
		std::cout << "Socket Initialization Error" << std::endl;
		return ;
	}

	// Create Socket  �����׽���

	SocketHandle = INVALID_SOCKET;//���ȴ���һ��SOCKET��� �׽��־��
	
	struct protoent*	pProtocolInfoEntry;//Э��ṹ�壬�������ƣ���������ŵ�
	char*				protocol;
	int					type;

	protocol = "tcp";//Э�������
	type = SOCK_STREAM;//socket������  TCP

	pProtocolInfoEntry = getprotobyname(protocol);//getprotobyname������ͨ��protocol�������õ�Э�����Ϣ�����ɹ������ظ�Э���һ��protoent�ṹ�壬ʧ�ܷ���NULL
	assert(pProtocolInfoEntry);  //void assert(int expression),������ʽexpression�������ֵΪ�٣�������stderr��ӡһ��������Ϣ��Ȼ�����abort������ֹ��������

	if(pProtocolInfoEntry)
		SocketHandle = socket(PF_INET, type, pProtocolInfoEntry->p_proto);//����socket(int af,int type,int protocol),af����ָ�������ַ���ͣ�type����ָ���׽�������
	                                                  //protocol����ָ������Э����TCP�����׽��ִ����ɹ����򷵻��������׽��ֵľ��SOCKET�����򷵻�INVALID_SOCKET����

	if(SocketHandle == INVALID_SOCKET)
	{
		std::cout << "Socket Creation Error" << std::endl;//û����ʾ�����׽��ִ����ɹ�
		return ;
	}

	//	Find Endpoint  Ѱ�Ҷ˵�
    	
 
	struct hostent*		pHostInfoEntry;  //�����ṹ�壬���������淶��������IP��ַ���ͣ�����IP�ĳ��ȵ���Ϣ
	struct sockaddr_in	Endpoint;        //�׽��ֵĵ�ַ��sockaddr_inָTCP/IPЭ���µĵ�ַ

	static const int port = 800;//����˿�ֵ

	memset(&Endpoint, 0, sizeof(Endpoint));//memset(void *s,int c;size_t n)  �ú�������Ϊ��s���ڴ���е�Ԫ�ض�����Ϊc,ע�Ⲣ���ǽ�s����ΪNULL
	Endpoint.sin_family	= AF_INET;//��ʾSOCKET����Internet��
	Endpoint.sin_port	= htons(port);//htons()�������ڽ������ֽ�˳��任Ϊ�����ֽ�˳��
	

	pHostInfoEntry = gethostbyname("192.168.1.102");  //�������������������õ�һ��hostent�ṹ�����������ʧ�ܣ���õ�NULL

	if(pHostInfoEntry)//�õ�һ��hostent���
		memcpy(&Endpoint.sin_addr,	pHostInfoEntry->h_addr, pHostInfoEntry->h_length);  //�ڴ濽���������������memset����
	else
		Endpoint.sin_addr.s_addr = inet_addr("192.168.1.102");//inet_addr�����ǽ�һ����ʽIP��ַת����Ϊһ��32λ���޷��ų�����

	if(Endpoint.sin_addr.s_addr == INADDR_NONE)
	{
		std::cout << "Bad Address" << std::endl;
		return ;
	}

	//	Create Socket

	int result = connect(	SocketHandle, (struct sockaddr*) & Endpoint, sizeof(Endpoint));
	/*����connect������ʵ�ֶ�һ���˵�����,��ͨ�����������������̷�����������int PASCAL FAR connect(SOCKET s�� const struct sockaddr FAR * name�� int namelen)��
	 ����s�����������ӵı����׽���������������nameָ��˵���Է��׽��ֵ�ַ�ṹ��ָ�롣
	 �Է��׽��ֵ�ַ������namelen˵�������û�д�������connect()����0�����򷵻�ֵSOCKET_ERROR*/
	if(result == SOCKET_ERROR)
	{
		std::cout << "Failed to create Socket" << std::endl;//Ӧ����û������
		int e = WSAGetLastError();
		return ;
	}

	//	A connection with the Vicon Realtime system is now open.��������ϣ�����Viconʵʱϵͳ�����ӿ�ͨ�����¼����Ƕ�ȡ����
	//	The following section implements the new Computer Graphics Client interface.
	/*	�ؼ���try��ʾ����һ���ܵ���ء��ܵ������ĳ������飻�ؼ���catch��tryң���Ӧ�����嵱try  block���ܼ�صĳ���飩�����쳣ʱ��
	������ĳ���ģ�飬����ÿ��catch  block����һ�������������ں�������ʱ����������������������������������쳣������������ͽ���ƥ�䣻
	��throw���Ǽ�⵽һ���쳣�������������׳�һ���쳣�¼���֪ͨ��Ӧ��catch�����ִ�ж�Ӧ�Ĵ�����  */
	try
	{
		info = new std::vector< std::string >;
		
		const int bufferSize = 2040;//�����С
		char buff[bufferSize];//���建����
		char * pBuff;
		


		//- Get Info - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		//	Request the channel information    ����ͨ����Ϣ

		pBuff = buff;
        //�����Ƕ���һ���������ݵİ�packet=KIND-TYPE-BODY
		* ((long int *) pBuff) = ClientCodes::EInfo;//kindΪ��Ϣ
		pBuff += sizeof(long int);
		* ((long int *) pBuff) = ClientCodes::ERequest;//typeΪ����
		pBuff += sizeof(long int);
		//send�������ط������ݵ��ֽ��������������󣬾ͷ���SOCKET_ERROR
		if(send(SocketHandle, buff, pBuff - buff, 0) == SOCKET_ERROR)//send(SOCKET s,const char* buf,int len,int flags)���ݷ��͵ĺ�����SOCKET�������ѽ������׽��֡�
			throw std::string("Error Requesting");                   //buf ���ַ���������������Ҫ���͵����ݣ�len��ָ��Ҫָ�����͵Ļ��������ַ�����
        
		//�����Ǽ���Ӧ�İ�
		long int packet;
		long int type;
		//��ΪKind �� Type ���ǳ����͵ģ�����ʹ�ó�������ƥ��
		if(!recieve(SocketHandle, packet))
			throw std::string("Error Recieving");

		if(!recieve(SocketHandle, type))
			throw std::string("Error Recieving");
		//
		if(type != ClientCodes::EReply)
			throw std::string("Bad Packet");

		if(packet != ClientCodes::EInfo)
			throw std::string("Bad Reply Type");
			
		
		long temp_size = size;
		if(!recieve(SocketHandle, temp_size))
			throw std::string();
		size = temp_size;

		info->resize(size);

		std::vector< std::string >::iterator iInfo;

		for(iInfo = info->begin(); iInfo != info->end(); iInfo++)
		{
			long int s;
			char c[255];
			char * p = c;

			if(!recieve(SocketHandle, s)) 
				throw std::string();

			if(!recieve(SocketHandle, c, s)) 
				throw std::string();
			
			p += s;
			
			*p = 0;
			
			*iInfo = std::string(c);
		}

		//- Parse Info - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		//	The info packets now contain the channel names.
		//	Identify the channels with the various dof's.
		
		MarkerChannels = new std::vector< MarkerChannel >;
		BodyChannels = new std::vector< BodyChannel >;

		for(iInfo = info->begin(); iInfo != info->end(); iInfo++)
		{
			//	Extract the channel type  ��ȡͨ������  CODE

			int openBrace = iInfo->find('<');//Ѱ��<����Ϊ������ͨ�����ĸ�ʽ��:NAME<CODE> 
			
			if(openBrace == iInfo->npos) 
				throw std::string("Bad Channel Id");
		
			int closeBrace = iInfo->find('>');
			
			if(closeBrace == iInfo->npos) 
				throw std::string("Bad Channel Id");

			closeBrace++;

			std::string Type = iInfo->substr(openBrace, closeBrace-openBrace);
            //������<>�е�CODE
			
			//	Extract the Name

			std::string Name = iInfo->substr(0, openBrace);
			//��Ϊ<ǰ��ľ���NAME

			int space = Name.rfind(' ');//rfind������� �������һ����str�е�ĳ���ַ�ƥ����ַ�����index��ʼ���ҡ����û�ҵ��ͷ���string::npos 
			
			if(space != Name.npos) 
				Name.resize(space);

			std::vector< MarkerChannel >::iterator iMarker;
			std::vector< BodyChannel >::iterator iBody;
			std::vector< std::string >::const_iterator iTypes;

			iMarker = std::find(	MarkerChannels->begin(), 
									MarkerChannels->end(), Name);

			iBody = std::find(BodyChannels->begin(), BodyChannels->end(), Name);

			if(iMarker != MarkerChannels->end())
			{
				//	The channel is for a marker we already have.
				iTypes = std::find(	ClientCodes::MarkerTokens.begin(), ClientCodes::MarkerTokens.end(), Type);
				if(iTypes != ClientCodes::MarkerTokens.end())
					iMarker->operator[](iTypes - ClientCodes::MarkerTokens.begin()) = iInfo - info->begin();
			}
			else
			if(iBody != BodyChannels->end())
			{
				//	The channel is for a body we already have.
				iTypes = std::find(ClientCodes::BodyTokens.begin(), ClientCodes::BodyTokens.end(), Type);
				if(iTypes != ClientCodes::BodyTokens.end())
					iBody->operator[](iTypes - ClientCodes::BodyTokens.begin()) = iInfo - info->begin();
			}
			else
			if((iTypes = std::find(ClientCodes::MarkerTokens.begin(), ClientCodes::MarkerTokens.end(), Type))
					!= ClientCodes::MarkerTokens.end())
			{
				//	Its a new marker.
				MarkerChannels->push_back(MarkerChannel(Name));
				MarkerChannels->back()[iTypes - ClientCodes::MarkerTokens.begin()] = iInfo - info->begin();
			}
			else
			if((iTypes = std::find(ClientCodes::BodyTokens.begin(), ClientCodes::BodyTokens.end(), Type))
					!= ClientCodes::BodyTokens.end())
			{
				//	Its a new body.
				BodyChannels->push_back(BodyChannel(Name));
				BodyChannels->back()[iTypes - ClientCodes::BodyTokens.begin()] = iInfo - info->begin();
			}
			else
			if(Type == "<F>")
			{
				FrameChannel = iInfo - info->begin();
			}
			else
			{
				//	It could be a new channel type.
			}

		}
		markerPositions = new std::vector< MarkerData >;
		markerPositions->resize(MarkerChannels->size());

		bodyPositions = new std::vector< BodyData >;
		bodyPositions->resize(BodyChannels->size());  

		data = new std::vector< double >;
		data->resize(info->size());

		pos = new struct position;
	}
	catch(const std::string & rMsg)
	{
		if(rMsg.empty())
			std::cout << "Error! Error! Error! Error! Error!" << std::endl;
		else
			std::cout << rMsg.c_str() << std::endl;
	}



	
}

void AR::Drone::Vicon::ViconClient::MyRecieve()
{
	try
	{
		const int bufferSize = 2040;//�����С
		char buff[bufferSize];//���建����
		char * pBuff;

		pBuff = buff;

	* ((long int *) pBuff) = ClientCodes::EData;          //���ݰ���KINDΪ2ʱ�����ݣ���EData=2
	pBuff += sizeof(long int);
	* ((long int *) pBuff) = ClientCodes::ERequest;       //���ݰ���TYPEΪ0ʱ�����󣬼�ERequest=0
	pBuff += sizeof(long int);

	if(send(SocketHandle, buff, pBuff - buff, 0) == SOCKET_ERROR)
		throw std::string("Error Requesting");

	long int packet;
	long int type;

	//	Get and check the packet header.    ��ȡ��������ݰ�ͷ

	if(!recieve(SocketHandle, packet))
		throw std::string("Error Recieving");

	if(!recieve(SocketHandle, type))
		throw std::string("Error Recieving");

	if(type != ClientCodes::EReply)
		throw std::string("Bad Packet");

	if(packet != ClientCodes::EData)
		throw std::string("Bad Reply Type");

	long temp_size = size;
	if(!recieve(SocketHandle, temp_size))
		throw std::string();
	size = temp_size;

	if(size != info->size())
		throw std::string("Bad Data Packet");

	//	Get the data.  ��ȡ����

	std::vector< double >::iterator iData;

	for(iData = data->begin(); iData != data->end(); iData++)
	{	
		if(!recieve(SocketHandle, *iData)) 
			throw std::string();
	}

	//- Look Up Channels - - - - - - - - - - - - - - - - - - - - - - - Ѱ��ͨ��
	//  Get the TimeStamp

	timestamp = data->at(FrameChannel);

	//	Get the channels corresponding to the markers.   �õ���Ӧ�ڱ�־��ͨ��
	//	Y is up   Y������
	//	The values are in millimeters  ��Щֵ�Ժ���Ϊ��λ

	std::vector< MarkerChannel >::iterator iMarker;
	std::vector< MarkerData >::iterator iMarkerData;
	int j = 0;
	for(	iMarker = MarkerChannels->begin(), 
		iMarkerData = markerPositions->begin(); 
		iMarker != MarkerChannels->end(); iMarker++, iMarkerData++)
	{

		iMarkerData->X = data->at(iMarker->X);
		iMarkerData->Y = data->at(iMarker->Y);
		iMarkerData->Z = data->at(iMarker->Z);

		if (j>=4) j = 0;
		else if(j == 0)
		{
			pos->times = timestamp;
			pos->X1 = iMarkerData->X;	
			pos->Y1 = iMarkerData->Y;
			pos->Z1 = iMarkerData->Z;
		}
		else if (j == 1)
		{
			pos->times = timestamp;
			pos->X2 = iMarkerData->X;	
			pos->Y2 = iMarkerData->Y;
			pos->Z2 = iMarkerData->Z;
		}
		else if(j == 2)
		{
			pos->times = timestamp;
			pos->X3 = iMarkerData->X;	
			pos->Y3 = iMarkerData->Y;
			pos->Z3 = iMarkerData->Z;
		}else if (j == 3)
		{
			pos->times = timestamp;
			pos->X4 = iMarkerData->X;	
			pos->Y4 = iMarkerData->Y;
			pos->Z4 = iMarkerData->Z;
		}
		else
		{

		}
		j++;


		//std::cout<<(iMarker->Name)<<":"<<std::endl;
		//std::cout<<"X:"<<(iMarkerData->X)<<std::endl;
		if(data->at(iMarker->O)> 0.5)
			iMarkerData->Visible = false;
		else
			iMarkerData->Visible = true;
	}
	//�����ǵõ�MARKERS����Ϣ������P-X,P-Y,P-Z,P-O��  λ����Ϣ

	//	Get the channels corresponding to the bodies.��Ӧ�ڸ����ͨ��
	//=================================================================
	//	The bodies are in global space  ���body����ȫ�ֿռ�
	//	The world is Z-up   �������ϵ��Z�ᳯ�ϵ�
	//	The translational values are in millimeters    ƽ��ֵ�Ǻ��׼�
	//	The rotational values are in radians ��תֵ���Ի��ȼ�
	//=================================================================

	std::vector< BodyChannel >::iterator iBody;
	std::vector< BodyData >::iterator iBodyData;
	//��Ҫ��õĸ�������Ϊƽ��ֵTX,TY,TZ����תֵQX,QY,QZ,QW����ת����ΪGlobalrotation[3][3]����ŷ���Ǳ�ʾ
	for(	iBody = BodyChannels->begin(), 
		iBodyData = bodyPositions->begin(); 
		iBody != BodyChannels->end(); iBody++, iBodyData++)
	{

		/*��Ҫ��õĸ�������Ϊƽ��ֵTX,TY,TZ*/
		iBodyData->TX = data->at(iBody->TX);
		iBodyData->TY = data->at(iBody->TY);
		iBodyData->TZ = data->at(iBody->TZ);


		pos->TX = iBodyData->TX;
		pos->TY = iBodyData->TY;
		pos->TZ = iBodyData->TZ;

		//	The channel data is in the angle-axis form.
		//	The following converts this to a quaternion.���½���ת��Ϊ��Ԫ��
		//=============================================================
		//	An angle-axis is vector, the direction of which is the axis
		//	of rotation and the length of which is the amount of 
		//	rotation in radians. ������һ���������䷽������ת�����䳤�����Ի��ȱ�ķ�תֵ
		//=============================================================

		double len, tmp;

		len = sqrt(	data->at(iBody->RX) * data->at(iBody->RX) + 
			data->at(iBody->RY) * data->at(iBody->RY) + 
			data->at(iBody->RZ) * data->at(iBody->RZ));

		iBodyData->QW = cos(len / 2.0);
		tmp = sin(len / 2.0);
		if (len < 1e-10) 
		{
			iBodyData->QX = data->at(iBody->RX);
			iBodyData->QY = data->at(iBody->RY);
			iBodyData->QZ = data->at(iBody->RZ);
		} 
		else 
		{
			iBodyData->QX = data->at(iBody->RX) * tmp/len;
			iBodyData->QY = data->at(iBody->RY) * tmp/len;
			iBodyData->QZ = data->at(iBody->RZ) * tmp/len;
		}
		/*�����Ƿ�תֵQX,QY,QZ,QW*/
		//	The following converts angle-axis to a rotation matrix.

		double c, s, x, y, z;

		if (len < 1e-15)
		{
			iBodyData->GlobalRotation[0][0] = iBodyData->GlobalRotation[1][1] = iBodyData->GlobalRotation[2][2] = 1.0;
			iBodyData->GlobalRotation[0][1] = iBodyData->GlobalRotation[0][2] = iBodyData->GlobalRotation[1][0] =
				iBodyData->GlobalRotation[1][2]	= iBodyData->GlobalRotation[2][0] = iBodyData->GlobalRotation[2][1] = 0.0;
		}
		else
		{
			x = data->at(iBody->RX)/len;
			y = data->at(iBody->RY)/len;
			z = data->at(iBody->RZ)/len;

			c = cos(len);
			s = sin(len);

			iBodyData->GlobalRotation[0][0] = c + (1-c)*x*x;
			iBodyData->GlobalRotation[0][1] =     (1-c)*x*y + s*(-z);
			iBodyData->GlobalRotation[0][2] =     (1-c)*x*z + s*y;
			iBodyData->GlobalRotation[1][0] =     (1-c)*y*x + s*z;
			iBodyData->GlobalRotation[1][1] = c + (1-c)*y*y;
			iBodyData->GlobalRotation[1][2] =     (1-c)*y*z + s*(-x);
			iBodyData->GlobalRotation[2][0] =     (1-c)*z*x + s*(-y);
			iBodyData->GlobalRotation[2][1] =     (1-c)*z*y + s*x;
			iBodyData->GlobalRotation[2][2] = c + (1-c)*z*z;
		}

		// now convert rotation matrix to nasty Euler angles (yuk)  ����ת����ת��Ϊŷ����
		// you could convert direct from angle-axis to Euler if you wish

		//	'Look out for angle-flips, Paul...'
		//  Algorithm: GraphicsGems II - Matrix Techniques VII.1 p 320
		assert(fabs(iBodyData->GlobalRotation[0][2]) <= 1);
		iBodyData->EulerY = asin(-iBodyData->GlobalRotation[2][0]);

		if(fabs(cos(y)) > 
			std::numeric_limits<double>::epsilon() ) 	// cos(y) != 0 Gimbal-Lock
		{
			iBodyData->EulerX = atan2(iBodyData->GlobalRotation[2][1], iBodyData->GlobalRotation[2][2]);
			iBodyData->EulerZ = atan2(iBodyData->GlobalRotation[1][0], iBodyData->GlobalRotation[0][0]);
		}
		else
		{
			iBodyData->EulerZ = 0;
			iBodyData->EulerX = atan2(iBodyData->GlobalRotation[0][1], iBodyData->GlobalRotation[1][1]);
		}

		pos->EX = iBodyData->EulerX;
		pos->EY = iBodyData->EulerY;
		pos->EZ = iBodyData->EulerZ;
		}


	cout<<pos->times<<endl;
	cout<<pos->TX<<endl<<endl;

}
catch(const std::string & rMsg)
{
	if(rMsg.empty())
		std::cout << "Error! Error! Error! Error! Error!" << std::endl;
	else
		std::cout << rMsg.c_str() << std::endl;
}


}

bool AR::Drone::Vicon::ViconClient::recieve(SOCKET Socket, char * pBuffer, int BufferSize)
{
	char * p = pBuffer;
	char * e = pBuffer + BufferSize;

	int result;

	while(p != e)
	{
		result = recv(	Socket, p, e - p, 0 );//���ۿͻ��˻��Ƿ���������recv��������TCP��һ�˽������ݣ�recv(SOCKET s,char *FAR buf,int len,int flags)
		//sָ�����ն��׽�����������bufָ��һ���������������ݣ�lenָ��buf�ĳ��ȣ����ĸ�����һ������Ϊ0��recv��������
		//��ʵ��copy���ֽ��������recv��copyʱ�����򷵻�SOCKET_ERROR.
		if(result == SOCKET_ERROR)
			return false;

		p += result;
	}

	return true;
}

bool AR::Drone::Vicon::ViconClient::recieve(SOCKET Socket, long int * Val)
{
	return recieve(Socket, (char*) Val, sizeof(Val));
}

bool AR::Drone::Vicon::ViconClient::recieve(SOCKET Socket, unsigned long int & Val)
{
	return recieve(Socket, (char*) & Val, sizeof(Val));
}

bool AR::Drone::Vicon::ViconClient::recieve(SOCKET Socket, double & Val)
{
	return recieve(Socket, (char*) & Val, sizeof(Val));
}

bool AR::Drone::Vicon::ViconClient::recieve(SOCKET Socket, long int & Val)
{
	return recieve(Socket, (char*) & Val, sizeof(Val));
}

void AR::Drone::Vicon::ViconClient::closeSocket()
{
	if(closesocket(SocketHandle) == SOCKET_ERROR)
	{
		std::cout << "Failed to close Socket" << std::endl;
		return ;
	}
}

position* AR::Drone::Vicon::ViconClient::getPos()
{
	return this->pos;
}
