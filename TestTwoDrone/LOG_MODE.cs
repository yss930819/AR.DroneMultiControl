/*
 * 作者 : 杨率帅
 * 时间 ： 2015 5 8
 * 版本 ： V1.0
 * Log打印模式定义
 * 
 */ 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestTwoDrone
{
    public enum LOG_MODE
    {
        /// <summary>
        /// 正常
        /// </summary>
        LOG_NORMAL = 0,
        /// <summary>
        /// 错误
        /// </summary>
        LOG_ERROR = 1,
        /// <summary>
        /// 警告
        /// </summary>
        LOG_WARNING = 2,
        /// <summary>
        /// 测试
        /// </summary>
        LOG_TEST = 3,

    }
}
