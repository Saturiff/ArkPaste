using System.Collections.Generic;

namespace WxBotDotNET.Models
{
    public class wxRequestParams
    {
        #region batch_get_group_members
        public class BaseRequestParam
        {
            public string Sid;
            public string Skey;
            public string DeviceID;
            public string Uin;
        }

        public class listParam
        {
            public string UserName;
            public string EncryChatRoomId;
        }

        public class GroupPostParams
        {
            public int Count;
            public BaseRequestParam BaseRequest;
            public List<listParam> List = new List<listParam>();
        }
        #endregion
    }
}
