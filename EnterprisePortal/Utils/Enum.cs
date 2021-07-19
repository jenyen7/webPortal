using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnterprisePortal.Utils
{
    public class Enum
    {
        public enum ActivityType
        {
            未分類 = 0,
            未分類_需身分證 = 1,
            旅遊_需身分證 = 2,
            娛樂 = 3,
            酒吧 = 4,
            聚餐 = 5,
            下午茶 = 6
        }

        public enum TransportType
        {
            自行前往 = 1,
            搭公司車 = 2,
            搭遊覽車 = 3,
            搭乘高鐵 = 4
        }

        public enum ListType
        {
            公告 = 0,
            待辦 = 1,
            活動 = 2,
            投票 = 3
        }

        public enum ToDoListStatus
        {
            待處理 = 0,
            處理中 = 1,
            已完成 = 2
        }

        public enum LogAction
        {
            登入 = 0,
            新增 = 1,
            修改 = 2,
            刪除 = 3
        }

        public enum LogArea
        {
            部門 = 0,
            權限 = 1,
            角色 = 2,
            帳號 = 3,
            公告 = 4,
            活動 = 5,
            投票 = 6,
            連結 = 7,
            連結分類 = 8,
            首頁登入 = 9
        }
    }
}