// AR.Drone.Vicon.h

#pragma once


using namespace System;

namespace AR { namespace Drone{
	namespace Vicon{
		public ref class ViconClient
		{
		public:
			void init();
			position *pos;
			void MyRecieve();
			void closeSocket();
			position* getPos();
		private :

			long size;
			std::vector< std::string > *info;
			std::vector< double > *data;


			double timestamp;

			std::vector< MarkerData > *markerPositions;    //±Í÷æŒª÷√
			

			std::vector< BodyData > *bodyPositions;     //
			
			std::vector< MarkerChannel >	*MarkerChannels;
			std::vector< BodyChannel >		*BodyChannels;

			int	FrameChannel;
			SOCKET	SocketHandle ;
			bool recieve(SOCKET Socket, char * pBuffer, int BufferSize);
			bool recieve(SOCKET Socket, long int * Val);
			bool recieve(SOCKET Socket, long int & Val);
			bool recieve(SOCKET Socket, unsigned long int & Val);
			bool recieve(SOCKET Socket, double & Val);
		};
	}

}

	
}
