/**********************************************************
*Author: TY
*http://www.fg-china.net   
**********************************************************/

#include "Stdafx.h"
#include <Winsock2.h>
#include <stdio.h>
#include<stdlib.h>
#include "quadrotor.h"
#include <windows.h>
#include "iostream"
using namespace std;

void main()
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

	SOCKET sockClient=socket(AF_INET,SOCK_DGRAM,0);
	SOCKADDR_IN addrSrv;
	//addrSrv.sin_addr.S_un.S_addr=inet_addr("192.168.0.10");  //不同主机之间发送数据
	addrSrv.sin_addr.S_un.S_addr=inet_addr("127.0.0.1");  //同一主机之间发送数据
	addrSrv.sin_family=AF_INET;
	addrSrv.sin_port=htons(5500);

/****************************************************************************/
    quadrotor fgbuf;

	fgbuf.version=FG_NET_FDM_VERSION;		// increment when data values change
	fgbuf.padding=0;		// padding

	// Positions
	fgbuf.longitude=0;		// geodetic (radians)
	fgbuf.latitude=0;		// geodetic (radians)
	fgbuf.altitude=0;		// above sea level (meters)
	fgbuf.agl=0;			// above ground level (meters)
	fgbuf.phi=0;			// roll (radians)
	fgbuf.theta=0;		// pitch (radians)
	fgbuf.psi=0;			// yaw or true heading (radians)
	fgbuf.alpha=0;                // angle of attack (radians)
	fgbuf.beta=0;                 // side slip angle (radians)

	// Velocities
	fgbuf.phidot=0;		// roll rate (radians/sec)
	fgbuf.thetadot=0;		// pitch rate (radians/sec)
	fgbuf.psidot=0;		// yaw rate (radians/sec)
	fgbuf.vcas=0;		        // calibrated airspeed
	fgbuf.climb_rate=0;		// feet per second
	fgbuf.v_north=0;              // north velocity in local/body frame, fps
	fgbuf.v_east=0;               // east velocity in local/body frame, fps
	fgbuf.v_down=0;               // down/vertical velocity in local/body frame, fps
	fgbuf.v_wind_body_north=0;    // north velocity in local/body frame
	// relative to local airmass, fps
	fgbuf.v_wind_body_east=0;     // east velocity in local/body frame
	// relative to local airmass, fps
	fgbuf.v_wind_body_down=0;     // down/vertical velocity in local/body
	// frame relative to local airmass, fps

	// Accelerations
	fgbuf.A_X_pilot=0;		// X accel in body frame ft/sec^2
	fgbuf.A_Y_pilot=0;		// Y accel in body frame ft/sec^2
	fgbuf.A_Z_pilot=0;		// Z accel in body frame ft/sec^2

	// Stall
	fgbuf.stall_warning=0;        // 0.0 - 1.0 indicating the amount of stall
	fgbuf.slip_deg=0;		// slip ball deflection

	// Pressure

	// Engine status
	fgbuf.num_engines=0;	     // Number of valid engines

	for (int i=0;i<4;i++)
	{
		fgbuf.eng_state[i]=0;// Engine state (off, cranking, running)
		fgbuf.rpm[i]=0;	     // Engine RPM rev/min
		fgbuf.fuel_flow[i]=0; // Fuel flow gallons/hr
		fgbuf.fuel_px[i]=0;   // Fuel pressure psi
		fgbuf.egt[i]=0;	     // Exhuast gas temp deg F
		fgbuf.cht[i]=0;	     // Cylinder head temp deg F
		fgbuf.mp_osi[i]=0;    // Manifold pressure
		fgbuf.tit[i]=0;	     // Turbine Inlet Temperature
		fgbuf.oil_temp[i]=0;  // Oil temp deg F
		fgbuf.oil_px[i]=0;    // Oil pressure psi
	}
	
	// Consumables
	fgbuf.num_tanks=0;		// Max number of fuel tanks

	for (int i=0;i<4;i++)
	{
         fgbuf.fuel_quantity[i]=0;
	}
	
	// Gear status
	for (int i=0;i<3;i++)
	{
		fgbuf.num_wheels;
		fgbuf.wow[i]=0;
		fgbuf.gear_pos[i]=0;
		fgbuf.gear_steer[i]=0;
		fgbuf.gear_compression[i]=0;
	}


	// Environment
	fgbuf.cur_time;           // current unix time
	// FIXME: make this uint64_t before 2038
	fgbuf.warp;                // offset in seconds to unix time
	fgbuf.visibility;            // visibility in meters (for env. effects)

	// Control surface positions (normalized values)
	fgbuf.elevator=0;
	fgbuf.elevator_trim_tab=0;
	fgbuf.left_flap=0;
	fgbuf.right_flap=0;
	fgbuf.left_aileron=0;
	fgbuf.right_aileron=0;
	fgbuf.rudder=0;
	fgbuf.nose_wheel=0;
	fgbuf.speedbrake=0;
	fgbuf.spoilers=0;

    int length=sizeof(fgbuf);

//从这开始编写
	double longitude=-2.13613;		// 设置经度初始值
	double latitude=0.65673;		// 设置维度初始值
	double altitude=5;		// 设置高度初始值
    float phi=0;			// 设置roll初始值
	float theta=0;		// 设置pitch初始值 
	float psi=-0.1;			// 设置yaw初始值
    float rpm[4];	     // 设置旋翼转速初始值
    for (int i=0;i<4;i++)
	{
		rpm[i]=400;
	}

loop:printf("1.键盘控制 2.文本控制\n");
char in;
in=getchar();
getchar();

//文本控制
if(in=='2')
{
//定义文本的10个数据
double n1;
double n2;
double n3;
double n4;
double x;
double y;
double z;
double phi_1;
double theta_1;
double psi_1;
double n5;
double n6;
double n7;
double n8;
double n9;
double n10;
 FILE *fp;
 if((fp=fopen("data.txt","r"))==NULL) //打开文本
{
	printf("cannot open file\n");
 }
int i;
for(i=0;i<3000;i++) //读取1000组数据
{
	 
        fscanf(fp,"%lf %lf %lf %lf %lf %lf %lf %lf %lf %lf %lf %lf %lf %lf %lf %lf",&n1,&n2,&n3,&n4,&x,&y,&z,&phi_1,&theta_1,&psi_1,&n5,&n6,&n7,&n8,&n9,&n10); //格式化读取数据
        printf("%4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f %4.3f\n",n1,n2,n3,n4,x,y,z,phi_1,theta_1,psi_1,n5,n6,n7,n8,n9,n10); //显示读取的数据
        //数据输入FG
		longitude=x/6371004-2.1361;
		latitude=y/6371004+0.65673;
		altitude=z+5;
		phi=phi_1;
		theta=theta_1;
		psi=psi_1;

		fgbuf.longitude=longitude;		
		fgbuf.latitude=latitude;		
		fgbuf.altitude=altitude;		
		fgbuf.phi=phi;
		fgbuf.theta=theta;
		fgbuf.psi=psi;
		for (int i=0;i<4;i++)
		{
			fgbuf.rpm[i]=rpm[i];
		}
        //数据格式转换
		fgbuf.ConverData();
		//发送数据
		sendto(sockClient,(char*)(&fgbuf),length,0,(SOCKADDR*)&addrSrv,sizeof(SOCKADDR));
	    Sleep(30);
  
}
getchar();
getchar();
fclose(fp); //关闭文本

}


//键盘控制
else if(in=='1')
{
	while(1)
	{
		Sleep(10);

		//检测键盘按下，如果按下，则小于0
		int A=GetAsyncKeyState(0x41);
		int S=GetAsyncKeyState(0x53);
		int W=GetAsyncKeyState(0x57);
		int D=GetAsyncKeyState(0x44);
		int UP=GetAsyncKeyState(0x26);
		int DOWN=GetAsyncKeyState(0x28);
		
		//经度变化
		if (A<0)
		{
			longitude=longitude-0.000000157;
		    phi=-0.5;
		}
		else if (D<0)
		{
			longitude=longitude+0.000000157;
			phi=0.5;
		}
		else
		{
			phi=0;
		}

		//维度变化
		if (S<0)
		{
			latitude=latitude-0.000000157;
			theta=0.5;
		}
		else if (W<0)
		{
			latitude=latitude+0.000000157;
			theta=-0.5;
		}
		else
		{
			theta=0;
		}

		//高度变化
		int Q=GetAsyncKeyState(0x51);
		if (Q<0)
			altitude=altitude+1;

		int E=GetAsyncKeyState(0x45);
		if (E<0)
			altitude=altitude-1;
		
		//旋翼转速变化
		if (UP<0)
			for (int i=0;i<4;i++)
	        {
		        rpm[i]=rpm[i]+50;
	        }
        
		if (DOWN<0)
			for (int i=0;i<4;i++)
	        {
		        rpm[i]=rpm[i]-50;
	        }


        //数据传送
		fgbuf.longitude=longitude;		
		fgbuf.latitude=latitude;		
		fgbuf.altitude=altitude;		
		fgbuf.phi=phi;
		fgbuf.theta=theta;
		fgbuf.psi=psi;
		for (int i=0;i<4;i++)
		{
			fgbuf.rpm[i]=rpm[i];
		}
	





		//数据格式转换
		fgbuf.ConverData();
		//发送数据
		sendto(sockClient,(char*)(&fgbuf),length,0,(SOCKADDR*)&addrSrv,sizeof(SOCKADDR));
		cout<<"Send Data!"<<endl;
		
	}
}
	

//输入不是1或2，返回重新输入
else
{
	printf("error 请重新输入\n");
	goto loop;
}




/****************************************************************************/
	closesocket(sockClient);
	WSACleanup();
}