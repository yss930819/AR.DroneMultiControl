using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AR.Drone.Data.Navigation.Native.Options
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public unsafe struct navdata_gps_t
    {
        public ushort tag;
        public ushort size;
        public double latitude;
        public double longitude;
        public double elevation;
        public double hdop;
        public int dataAvailable;
        public int unk_0;
        public int unk_112;
        public double lat0;
        public double lon0;
        public double latFuse;
        public double lonFuse;
        public uint gpsState;
        public fixed int unk_1[5];
        public double vdop;                   /*!< vdop */
        public double pdop;                   /*!< pdop */
        public float speed;                  /*!< speed */
        public uint last_frame_timestamp;   /*!< Timestamp from the last frame */
        public float degree;                 /*!< Degree */
        public float degree_mag;             /*!< Degree of the magnetic */
        public fixed byte unk_2[16];
        //  struct{
        //    uint8_t     sat;
        //    uint8_t     cn0;
        //  }channels[12];
        public fixed ushort sat_channel[12];		/*!< Combined Sattelite and channel into one integer */
        public int gps_plugged;            /*!< When the gps is plugged */
        public fixed byte unk_3[108];
        public double gps_time;               /*!< The gps time of week */
        public ushort week;                   /*!< The gps week */
        public byte gps_fix;                /*!< The gps fix */
        public byte num_sattelites;         /*!< Number of sattelites */
        public fixed byte unk_4[24];
        public double ned_vel_c0;             /*!< NED velocity */
        public double ned_vel_c1;             /*!< NED velocity */
        public double ned_vel_c2;             /*!< NED velocity */
        public double pos_accur_c0;           /*!< Position accuracy */
        public double pos_accur_c1;           /*!< Position accuracy */
        public double pos_accur_c2;           /*!< Position accuracy */
        public float speed_acur;             /*!< Speed accuracy */
        public float time_acur;              /*!< Time accuracy */
        public fixed byte unk_5[72];
        public float temprature;
        public float pressure;

    }
}
