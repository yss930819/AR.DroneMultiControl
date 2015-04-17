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



class ClientCodes         //客户端代码  ——这就是协议中的包(Packet: Kind-Type-Body)
{
public:
	//enum枚举类型，适用于事件只有有限个已知的可能值，将其罗列出来
	//对枚举元素按常量处理，不能对它们赋值，即它们有默认值（从0开始），当然，默认值可以自定，正好符合协议定义
	enum EType		//Type   类型
	{
		ERequest, 
		EReply
	};

	enum EPacket	//Kind  种类
	{
		EClose, 
		EInfo, 
		EData, 
		EStreamOn, 
		EStreamOff
	};

	static const std::vector< std::string > MarkerTokens;    //标志点标记 简单的说 std::vector 是一个动态数组，管理的是一块线性的、可动态增长的内存
	static const std::vector< std::string > BodyTokens;      //刚体标记

	static std::vector< std::string > MakeMarkerTokens()     //实现标志点标记
	{
		std::vector< std::string > v;
		v.push_back("<P-X>");   //push_back(const T& x) functions:add element at the end
		v.push_back("<P-Y>");   //P-X沿x轴方向的位置
		v.push_back("<P-Z>");   
		v.push_back("<P-O>");   //标志是否封闭
		return v;
	}

	static std::vector< std::string > MakeBodyTokens()        //实现刚体标记
	{
		std::vector< std::string > v;
		v.push_back("<A-X>");   //绕X轴的角度轴(angle-axis)旋转
		v.push_back("<A-Y>");
		v.push_back("<A-Z>");
		v.push_back("<T-X>");   //沿x轴的平移
		v.push_back("<T-Y>");
		v.push_back("<T-Z>");
		return v;
	}

	struct CompareNames : std::binary_function<std::string, std::string, bool>     //创建一个CompareNames的结构体  功能： 比较名称
	{
		bool operator()(const std::string & a_S1, const std::string & a_S2) const  
		{
			std::string::const_iterator iS1 = a_S1.begin();//const_iterator 迭代器，遍历容器内的元素，并访问这些元素，iterator可以更改这些元素值
			std::string::const_iterator iS2 = a_S2.begin();//但const_iterator不能更改

			while(iS1 != a_S1.end() && iS2 != a_S2.end())
				if(toupper(*(iS1++)) != toupper(*(iS2++))) return false;//toupper函数功能是把字符转换为大写字母

			return a_S1.size() == a_S2.size();
		}
	};



};

class MarkerChannel     //标志通道类
{
public:
	std::string Name;//名称

	int X;
	int Y;
	int Z;
	int O;

	MarkerChannel(std::string & a_rName) : X(-1), Y(-1), Z(-1), O(-1), Name(a_rName) {}   //类的初始化，构造函数

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
		return comparitor(Name, a_rName);  //比较名称是否符合
	}

};


class MarkerData    //标志数据
{
public:
	double	X;
	double	Y;
	double	Z;
	bool	Visible;
};

class BodyChannel   //刚体通道类
{
public:
	std::string Name;   //名

	int TX;
	int TY;
	int TZ;
	int RX;
	int RY;
	int RZ;

	BodyChannel(std::string & a_rName) : RX(-1), RY(-1), RZ(-1), TX(-1), TY(-1), TZ(-1), Name(a_rName) {}     //构造函数

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
		default:	assert(false); return TZ;//assert（expression）计算表达式的值，若为假则输出打印错误信息
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

class BodyData         //刚体数据
{
public:
	// Representation of body translation   body转移的表述
	double	TX;
	double	TY;
	double	TZ;//以上是刚体的转移信息

	// Representation of body rotation    body 翻转的表述
	// Quaternion 四元数
	double	QX;
	double	QY;
	double	QZ;
	double	QW;//以上是刚体翻转的四元数表示信息
	// Global rotation matrix   全局翻转矩阵
	double GlobalRotation[3][3];
   //欧拉角
	double EulerX;
	double EulerY;
	double EulerZ;//以上是翻转的欧拉角表示
};