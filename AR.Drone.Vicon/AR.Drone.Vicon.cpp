// 这是主 DLL 文件。
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
	//- Initialisation - - - - - 初始化 - - - - - - - - - - - -
	//  Windows-specific initialisation.

	WORD wVersionRequested;//参数wVersionRequested用于指定准备加载的Winsock库的版本：通常做法是高字节指定所需要的Winsock库的副版本，低字节指定主版本
	WSADATA wsaData;
	wVersionRequested = MAKEWORD( 2, 0 ); //用宏 MAKEWORD(X,Y)来指定wVersionRequested的正确值(X是高位字节，Y是低位字节)
	if(WSAStartup( wVersionRequested, &wsaData ) != 0)    //WSAStartup函数是用来对Winsock DLL进行初始化，协商Winsock 的版本支持，并分配必要的资源
	{
		std::cout << "Socket Initialization Error" << std::endl;
		return ;
	}

	// Create Socket  创建套接字

	SocketHandle = INVALID_SOCKET;//首先创建一个SOCKET句柄 套接字句柄
	
	struct protoent*	pProtocolInfoEntry;//协议结构体，包括名称，别名，编号等
	char*				protocol;
	int					type;

	protocol = "tcp";//协议的名称
	type = SOCK_STREAM;//socket的类型  TCP

	pProtocolInfoEntry = getprotobyname(protocol);//getprotobyname函数，通过protocol名称来得到协议的信息，若成功，返回该协议的一个protoent结构体，失败返回NULL
	assert(pProtocolInfoEntry);  //void assert(int expression),计算表达式expression，如果其值为假，则先向stderr打印一条出错信息，然后调用abort函数终止程序运行

	if(pProtocolInfoEntry)
		SocketHandle = socket(PF_INET, type, pProtocolInfoEntry->p_proto);//调用socket(int af,int type,int protocol),af用于指定网络地址类型；type用于指定套接字类型
	                                                  //protocol用于指定网络协议如TCP。若套接字创建成功，则返回所创建套接字的句柄SOCKET，否则返回INVALID_SOCKET错误

	if(SocketHandle == INVALID_SOCKET)
	{
		std::cout << "Socket Creation Error" << std::endl;//没有显示表明套接字创建成功
		return ;
	}

	//	Find Endpoint  寻找端点
    	
 
	struct hostent*		pHostInfoEntry;  //主机结构体，包括主机规范名，主机IP地址类型，主机IP的长度等信息
	struct sockaddr_in	Endpoint;        //套接字的地址，sockaddr_in指TCP/IP协议下的地址

	static const int port = 800;//服务端口值

	memset(&Endpoint, 0, sizeof(Endpoint));//memset(void *s,int c;size_t n)  该函数作用为把s的内存块中的元素都设置为c,注意并不是将s设置为NULL
	Endpoint.sin_family	= AF_INET;//表示SOCKET处于Internet域
	Endpoint.sin_port	= htons(port);//htons()函数用于将主机字节顺序变换为网络字节顺序
	

	pHostInfoEntry = gethostbyname("192.168.1.102");  //根据域名或主机名来得到一个hostent结构句柄，若返回失败，则得到NULL

	if(pHostInfoEntry)//得到一个hostent句柄
		memcpy(&Endpoint.sin_addr,	pHostInfoEntry->h_addr, pHostInfoEntry->h_length);  //内存拷贝，其参数定义与memset类似
	else
		Endpoint.sin_addr.s_addr = inet_addr("192.168.1.102");//inet_addr函数是将一个点式IP地址转换成为一个32位的无符号长整数

	if(Endpoint.sin_addr.s_addr == INADDR_NONE)
	{
		std::cout << "Bad Address" << std::endl;
		return ;
	}

	//	Create Socket

	int result = connect(	SocketHandle, (struct sockaddr*) & Endpoint, sizeof(Endpoint));
	/*调用connect函数来实现对一个端的连接,可通过其来进行向服务进程发送连接请求int PASCAL FAR connect(SOCKET s， const struct sockaddr FAR * name， int namelen)；
	 参数s是欲建立连接的本地套接字描述符。参数name指出说明对方套接字地址结构的指针。
	 对方套接字地址长度由namelen说明。如果没有错误发生，connect()返回0。否则返回值SOCKET_ERROR*/
	if(result == SOCKET_ERROR)
	{
		std::cout << "Failed to create Socket" << std::endl;//应该是没连接上
		int e = WSAGetLastError();
		return ;
	}

	//	A connection with the Vicon Realtime system is now open.如果连接上，则与Vicon实时系统的连接开通，以下即将是读取数据
	//	The following section implements the new Computer Graphics Client interface.
	/*	关键字try表示定义一个受到监控、受到保护的程序代码块；关键字catch与try遥相呼应，定义当try  block（受监控的程序块）出现异常时，
	错误处理的程序模块，并且每个catch  block都带一个参数（类似于函数定义时的数那样），这个参数的数据类型用于异常对象的数据类型进行匹配；
	而throw则是检测到一个异常错误发生后向外抛出一个异常事件，通知对应的catch程序块执行对应的错误处理。  */
	try
	{
		info = new std::vector< std::string >;
		
		const int bufferSize = 2040;//缓存大小
		char buff[bufferSize];//定义缓冲区
		char * pBuff;
		


		//- Get Info - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		//	Request the channel information    请求通道信息

		pBuff = buff;
        //以下是定义一个请求数据的包packet=KIND-TYPE-BODY
		* ((long int *) pBuff) = ClientCodes::EInfo;//kind为信息
		pBuff += sizeof(long int);
		* ((long int *) pBuff) = ClientCodes::ERequest;//type为请求
		pBuff += sizeof(long int);
		//send函数返回发送数据的字节数，若发生错误，就返回SOCKET_ERROR
		if(send(SocketHandle, buff, pBuff - buff, 0) == SOCKET_ERROR)//send(SOCKET s,const char* buf,int len,int flags)数据发送的函数，SOCKET参数是已建立的套接字。
			throw std::string("Error Requesting");                   //buf 是字符缓冲区，包含将要发送的数据；len是指将要指定发送的缓冲区的字符数；
        
		//以下是检测回应的包
		long int packet;
		long int type;
		//因为Kind 和 Type 都是长整型的，所以使用长整型来匹配
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
			//	Extract the channel type  提取通道类型  CODE

			int openBrace = iInfo->find('<');//寻找<是因为定义中通道名的格式是:NAME<CODE> 
			
			if(openBrace == iInfo->npos) 
				throw std::string("Bad Channel Id");
		
			int closeBrace = iInfo->find('>');
			
			if(closeBrace == iInfo->npos) 
				throw std::string("Bad Channel Id");

			closeBrace++;

			std::string Type = iInfo->substr(openBrace, closeBrace-openBrace);
            //以上是<>中的CODE
			
			//	Extract the Name

			std::string Name = iInfo->substr(0, openBrace);
			//因为<前面的就是NAME

			int space = Name.rfind(' ');//rfind反向查找 返回最后一个与str中的某个字符匹配的字符，从index开始查找。如果没找到就返回string::npos 
			
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
		const int bufferSize = 2040;//缓存大小
		char buff[bufferSize];//定义缓冲区
		char * pBuff;

		pBuff = buff;

	* ((long int *) pBuff) = ClientCodes::EData;          //数据包中KIND为2时是数据，即EData=2
	pBuff += sizeof(long int);
	* ((long int *) pBuff) = ClientCodes::ERequest;       //数据包中TYPE为0时是请求，即ERequest=0
	pBuff += sizeof(long int);

	if(send(SocketHandle, buff, pBuff - buff, 0) == SOCKET_ERROR)
		throw std::string("Error Requesting");

	long int packet;
	long int type;

	//	Get and check the packet header.    获取并检测数据包头

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

	//	Get the data.  获取数据

	std::vector< double >::iterator iData;

	for(iData = data->begin(); iData != data->end(); iData++)
	{	
		if(!recieve(SocketHandle, *iData)) 
			throw std::string();
	}

	//- Look Up Channels - - - - - - - - - - - - - - - - - - - - - - - 寻找通道
	//  Get the TimeStamp

	timestamp = data->at(FrameChannel);

	//	Get the channels corresponding to the markers.   得到对应于标志的通道
	//	Y is up   Y方向朝上
	//	The values are in millimeters  这些值以毫米为单位

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
	//以上是得到MARKERS的信息，包含P-X,P-Y,P-Z,P-O，  位置信息

	//	Get the channels corresponding to the bodies.对应于刚体的通道
	//=================================================================
	//	The bodies are in global space  这个body是在全局空间
	//	The world is Z-up   这个坐标系是Z轴朝上的
	//	The translational values are in millimeters    平移值是毫米记
	//	The rotational values are in radians 翻转值是以弧度记
	//=================================================================

	std::vector< BodyChannel >::iterator iBody;
	std::vector< BodyData >::iterator iBodyData;
	//所要获得的刚体数据为平移值TX,TY,TZ，翻转值QX,QY,QZ,QW，翻转矩阵为Globalrotation[3][3]，和欧拉角表示
	for(	iBody = BodyChannels->begin(), 
		iBodyData = bodyPositions->begin(); 
		iBody != BodyChannels->end(); iBody++, iBodyData++)
	{

		/*所要获得的刚体数据为平移值TX,TY,TZ*/
		iBodyData->TX = data->at(iBody->TX);
		iBodyData->TY = data->at(iBody->TY);
		iBodyData->TZ = data->at(iBody->TZ);


		pos->TX = iBodyData->TX;
		pos->TY = iBodyData->TY;
		pos->TZ = iBodyData->TZ;

		//	The channel data is in the angle-axis form.
		//	The following converts this to a quaternion.以下将其转换为四元数
		//=============================================================
		//	An angle-axis is vector, the direction of which is the axis
		//	of rotation and the length of which is the amount of 
		//	rotation in radians. 角轴是一个向量，其方向是旋转方向，其长度是以弧度标的翻转值
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
		/*以上是翻转值QX,QY,QZ,QW*/
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

		// now convert rotation matrix to nasty Euler angles (yuk)  将翻转矩阵转化为欧拉角
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
		result = recv(	Socket, p, e - p, 0 );//无论客户端还是服务器都用recv函数来从TCP另一端接受数据，recv(SOCKET s,char *FAR buf,int len,int flags)
		//s指定接收端套接字描述符，buf指定一个缓冲区接受数据，len指明buf的长度，第四个参数一般设置为0，recv函数返回
		//其实际copy的字节数，如果recv在copy时出错，则返回SOCKET_ERROR.
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
