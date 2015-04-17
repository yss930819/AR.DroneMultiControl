//-----------------------------------------------------------------------------
//	ClientCodes
//-----------------------------------------------------------------------------

#pragma once
#include <cassert>
#include <string>
#include <vector>
#include <functional>



	public struct position
	{
		double times;
		double X1;
		double Y1;
		double Z1;
		double X2;
		double Y2;
		double Z2;
		double X3;
		double Y3;
		double Z3;
		double X4;
		double Y4;
		double Z4;
		double TX;
		double TY;
		double TZ;
		double EX;
		double EY;
		double EZ;
	};



class ClientCodes         //�ͻ��˴���  ���������Э���еİ�(Packet: Kind-Type-Body)
{
public:
	//enumö�����ͣ��������¼�ֻ�����޸���֪�Ŀ���ֵ���������г���
	//��ö��Ԫ�ذ������������ܶ����Ǹ�ֵ����������Ĭ��ֵ����0��ʼ������Ȼ��Ĭ��ֵ�����Զ������÷���Э�鶨��
	enum EType		//Type   ����
	{
		ERequest, 
		EReply
	};

	enum EPacket	//Kind  ����
	{
		EClose, 
		EInfo, 
		EData, 
		EStreamOn, 
		EStreamOff
	};

	static const std::vector< std::string > MarkerTokens;    //��־���� �򵥵�˵ std::vector ��һ����̬���飬�������һ�����Եġ��ɶ�̬�������ڴ�
	static const std::vector< std::string > BodyTokens;      //������

	static std::vector< std::string > MakeMarkerTokens()     //ʵ�ֱ�־����
	{
		std::vector< std::string > v;
		v.push_back("<P-X>");   //push_back(const T& x) functions:add element at the end
		v.push_back("<P-Y>");   //P-X��x�᷽���λ��
		v.push_back("<P-Z>");   
		v.push_back("<P-O>");   //��־�Ƿ���
		return v;
	}

	static std::vector< std::string > MakeBodyTokens()        //ʵ�ָ�����
	{
		std::vector< std::string > v;
		v.push_back("<A-X>");   //��X��ĽǶ���(angle-axis)��ת
		v.push_back("<A-Y>");
		v.push_back("<A-Z>");
		v.push_back("<T-X>");   //��x���ƽ��
		v.push_back("<T-Y>");
		v.push_back("<T-Z>");
		return v;
	}

	struct CompareNames : std::binary_function<std::string, std::string, bool>     //����һ��CompareNames�Ľṹ��  ���ܣ� �Ƚ�����
	{
		bool operator()(const std::string & a_S1, const std::string & a_S2) const  
		{
			std::string::const_iterator iS1 = a_S1.begin();//const_iterator �����������������ڵ�Ԫ�أ���������ЩԪ�أ�iterator���Ը�����ЩԪ��ֵ
			std::string::const_iterator iS2 = a_S2.begin();//��const_iterator���ܸ���

			while(iS1 != a_S1.end() && iS2 != a_S2.end())
				if(toupper(*(iS1++)) != toupper(*(iS2++))) return false;//toupper���������ǰ��ַ�ת��Ϊ��д��ĸ

			return a_S1.size() == a_S2.size();
		}
	};



};

class MarkerChannel     //��־ͨ����
{
public:
	std::string Name;//����

	int X;
	int Y;
	int Z;
	int O;

	MarkerChannel(std::string & a_rName) : X(-1), Y(-1), Z(-1), O(-1), Name(a_rName) {}   //��ĳ�ʼ�������캯��

	int & operator[](int i)
	{
		switch(i)
		{
		case 0:		return X;
		case 1:		return Y;
		case 2:		return Z;
		case 3:		return O;
		default:	assert(false); return O;
		}
	}

	int operator[](int i) const
	{
		switch(i)
		{
		case 0:		return X;
		case 1:		return Y;
		case 2:		return Z;
		case 3:		return O;
		default:	assert(false); return -1;
		}
	}


	bool operator==(const std::string & a_rName) 
	{
		ClientCodes::CompareNames comparitor;
		return comparitor(Name, a_rName);  //�Ƚ������Ƿ����
	}

};


class MarkerData    //��־����
{
public:
	double	X;
	double	Y;
	double	Z;
	bool	Visible;
};

class BodyChannel   //����ͨ����
{
public:
	std::string Name;   //��

	int TX;
	int TY;
	int TZ;
	int RX;
	int RY;
	int RZ;

	BodyChannel(std::string & a_rName) : RX(-1), RY(-1), RZ(-1), TX(-1), TY(-1), TZ(-1), Name(a_rName) {}     //���캯��

	int & operator[](int i)
	{
		switch(i)
		{
		case 0:		return RX;
		case 1:		return RY;
		case 2:		return RZ;
		case 3:		return TX;
		case 4:		return TY;
		case 5:		return TZ;
		default:	assert(false); return TZ;//assert��expression��������ʽ��ֵ����Ϊ���������ӡ������Ϣ
		}
	}

	int operator[](int i) const
	{
		switch(i)
		{
		case 0:		return RX;
		case 1:		return RY;
		case 2:		return RZ;
		case 3:		return TX;
		case 4:		return TY;
		case 5:		return TZ;
		default:	assert(false); return -1;
		}
	}

	bool operator==(const std::string & a_rName) 
	{
		ClientCodes::CompareNames comparitor;
		return comparitor(Name, a_rName);
	}
};

class BodyData         //��������
{
public:
	// Representation of body translation   bodyת�Ƶı���
	double	TX;
	double	TY;
	double	TZ;//�����Ǹ����ת����Ϣ

	// Representation of body rotation    body ��ת�ı���
	// Quaternion ��Ԫ��
	double	QX;
	double	QY;
	double	QZ;
	double	QW;//�����Ǹ��巭ת����Ԫ����ʾ��Ϣ
	// Global rotation matrix   ȫ�ַ�ת����
	double GlobalRotation[3][3];
   //ŷ����
	double EulerX;
	double EulerY;
	double EulerZ;//�����Ƿ�ת��ŷ���Ǳ�ʾ
};