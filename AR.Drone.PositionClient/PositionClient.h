// AR.Drone.PositionClient.h

#pragma once

using namespace System;

namespace AR{
	namespace Drone{

		namespace Client{

			public ref class PositionClient
			{
			public :
				quadrotor* _quadrotor;
				PositionClient();
				~PositionClient();
				void RecevieData();
				void initSocket();
				double getlongitude();
				double getlatitude();
				double getpsi();
			private:
				SOCKET sockSrv;
				int len;
				int length;
				SOCKADDR_IN* addrClient;
			};
		}

	}
}
