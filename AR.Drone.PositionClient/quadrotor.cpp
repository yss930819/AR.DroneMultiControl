#include "quadrotor.h"



quadrotor::quadrotor(void)
{
}

quadrotor::~quadrotor(void)
{
}
void quadrotor::htond (double &x)	
{
	int    *Double_Overlay;
	int     Holding_Buffer;

	Double_Overlay = (int *) &x;
	Holding_Buffer = Double_Overlay [0];

	Double_Overlay [0] = htonl (Double_Overlay [1]);
	Double_Overlay [1] = htonl (Holding_Buffer);
}

//Float version
void quadrotor::htonf (float &x)	
{
	int    *Float_Overlay;
	int     Holding_Buffer;

	Float_Overlay = (int *) &x;
	Holding_Buffer = Float_Overlay [0];

	Float_Overlay [0] = htonl (Holding_Buffer);
}
//主机字节网络字节互换
void quadrotor::ConverData()
{
	    quadrotor *net=this;
		// Convert the net buffer to network format
		net->version = htonl(net->version);

		htond(net->longitude);
		htond(net->latitude);
		htond(net->altitude);
		htonf(net->agl);
		htonf(net->phi);
		htonf(net->theta);
		htonf(net->psi);
		htonf(net->alpha);
		htonf(net->beta);

		htonf(net->phidot);
		htonf(net->thetadot);
		htonf(net->psidot);
		htonf(net->vcas);
		htonf(net->climb_rate);
		htonf(net->v_north);
		htonf(net->v_east);
		htonf(net->v_down);
		htonf(net->v_wind_body_north);
		htonf(net->v_wind_body_east);
		htonf(net->v_wind_body_down);

		htonf(net->A_X_pilot);
		htonf(net->A_Y_pilot);
		htonf(net->A_Z_pilot);

		htonf(net->stall_warning);
		htonf(net->slip_deg);

		for (int i = 0; i < FG_MAX_ENGINES; ++i ) {
			net->eng_state[i] = htonl(net->eng_state[i]);
			htonf(net->rpm[i]);
			htonf(net->fuel_flow[i]);
			htonf(net->fuel_px[i]);
			htonf(net->egt[i]);
			htonf(net->cht[i]);
			htonf(net->mp_osi[i]);
			htonf(net->tit[i]);
			htonf(net->oil_temp[i]);
			htonf(net->oil_px[i]);
		}
		net->num_engines = htonl(FG_MAX_ENGINES);

		for (int i = 0; i < FG_MAX_TANKS; ++i ) {
			htonf(net->fuel_quantity[i]);
		}
		net->num_tanks = htonl(FG_MAX_TANKS);

		for (int i = 0; i < FG_MAX_WHEELS; ++i ) {
			net->wow[i] = htonl(net->wow[i]);
			htonf(net->gear_pos[i]);
			htonf(net->gear_steer[i]);
			htonf(net->gear_compression[i]);
		}
		net->num_wheels = htonl(FG_MAX_WHEELS);

		net->cur_time = htonl( net->cur_time );
		net->warp = htonl( net->warp );
		htonf(net->visibility);

		htonf(net->elevator);
		htonf(net->elevator_trim_tab);
		htonf(net->left_flap);
		htonf(net->right_flap);
		htonf(net->left_aileron);
		htonf(net->right_aileron);
		htonf(net->rudder);
		htonf(net->nose_wheel);
		htonf(net->speedbrake);
		htonf(net->spoilers);
}