// AR.Drone.PositionClient.h
#include "Stdafx.h"

#pragma once

using namespace System;

namespace AR{
	namespace Drone{

		namespace Client{
			public struct diyPosition
			{
				double x;
				double y;
				double z;
			};

			public ref class PositionClient
			{
			public :
				quadrotor* _quadrotor;
				PositionClient();
				~PositionClient();
				void RecevieData();
				void initSocket();
				diyPosition *position;
				diyPosition* getPosition1();
				diyPosition* getPosition2();
			private:
				SOCKET sockSrv;
				int len;
				int length;
				SOCKADDR_IN* addrClient;
			};
		}

	}
}
