using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace WxBotDotNET
{
    public class WxBot
    {
        private readonly string UNKONWN = "unkonwn";
        private readonly string SUCCESS = "200";
        private readonly string SCANED = "201";
        private readonly string TIMEOUT = "408";

        private string uuid = "";
        private string redirectUri = ""; // 不要使用全部取代，字串會影響資料傳遞
        private string baseUri = "";
        private string baseHost = "";
        private string uin = "";
        private string sid = "";
        private string skey = "";
        private string passTicket = ""; // 不要使用全部取代，字串會影響資料傳遞
        private string deviceId = "e" + new Random().NextDouble().ToString("f16").Replace(".", string.Empty).Substring(1);     // 'e' + repr(random.random())[2:17]
        private string baseRequest = "";
        private string baseRequest2 = "";

        private static Dictionary<string, string> dicSyncKey = new Dictionary<string, string>();
        private string sync_key_str = "";

        private wxContact me;    // 当前登录微信用户
        private List<Object> contactList = new List<object>();   //联系人
        private List<Object> groupList = new List<object>();   //群聊
        private List<Object> normalList = new List<object>(); //除去多人聊天以外的联系人
        private List<Models.wxGroup> groupinfoList = new List<Models.wxGroup>(); //群聊信息

        private static Dictionary<string, string> encryChatRoomIdList = new Dictionary<string, string>(); //用自字典存储方便查找

        public WxBot()
        {
            Trace.WriteLine("微信: 建構子");
        }


        #region run
        /// <summary>
        /// 主逻辑
        /// </summary>
        public bool Run()
        {
            Trace.WriteLine("微信: 主邏輯開始");
            Trace.WriteLine("微信: 主邏輯 -> 取得uuid");
            //获取uuid
            if (!GetUuid())
            {
                Trace.WriteLine("微信: 主邏輯 -> 取得uuid..失敗");
                return false;
            }

            //获取登录二维码
            Trace.WriteLine("微信: 主邏輯 -> 取得QR Code");
            GenQrCode();

            //等待扫描检测
            Trace.WriteLine("=====重點區域=====");
            Trace.WriteLine("微信: 主邏輯 -> 等待檢測");
            string result = Wait4Login();
            if (result != SUCCESS)
            {
                Trace.WriteLine("微信: 主邏輯 -> 等待檢測..失敗");
                return false;
            }

            Trace.WriteLine("微信: 主邏輯 -> 取得登入參數");
            //获取skey sid uid pass_ticket
            if (!Login())
            {
                Trace.WriteLine("微信: 主邏輯 -> 取得登入參數..失敗");
                return false;
            }

            Trace.WriteLine("微信: 主邏輯 -> 初始化參數");
            //初始化
            if (!Init())
            {
                Trace.WriteLine("微信: 主邏輯 -> 初始化參數..失敗");
                return false;
            }

            Trace.WriteLine("微信: 主邏輯 -> 取得聯絡人");
            //获取联系人
            GetContact();

            Trace.WriteLine("微信: 主邏輯結束");
            return true;
        }

        /// <summary>
        /// 获取当前账户的所有相关账号(包括联系人、公众号、群聊、特殊账号)
        /// </summary>
        public void GetContact()
        {

            contactList.Clear();
            groupList.Clear();
            normalList.Clear();
            encryChatRoomIdList.Clear();

            string[] special_users = {"newsapp", "fmessage", "filehelper", "weibo", "qqmail",
                                        "fmessage", "tmessage", "qmessage", "qqsync", "floatbottle",
                                        "lbsapp", "shakeapp", "medianote", "qqfriend", "readerapp",
                                        "blogapp", "facebookapp", "masssendapp", "meishiapp",
                                        "feedsapp", "voip", "blogappweixin", "weixin", "brandsessionholder",
                                        "weixinreminder", "wxid_novlwrv3lqwv11", "gh_22b87fa7cb3c",
                                        "officialaccounts", "notification_messages", "wxid_novlwrv3lqwv11",
                                        "gh_22b87fa7cb3c", "wxitil", "userexperience_alarm", "notification_messages"};

            string contact_str = Http.WebGet(baseUri + "/webwxgetcontact?pass_ticket=" + passTicket + "&skey=" + skey + "&r=" + Common.ConvertDateTimeToInt(DateTime.Now));
            JObject contact_result = JsonConvert.DeserializeObject(contact_str) as JObject;
            if (contact_result != null)
            {
                foreach (JObject contact in contact_result["MemberList"])  //完整好友名单
                {
                    wxContact user = new wxContact();
                    user.UserName = contact["UserName"].ToString();
                    user.City = contact["City"].ToString();
                    user.HeadImgUrl = contact["HeadImgUrl"].ToString();
                    user.NickName = contact["NickName"].ToString();
                    user.Province = contact["Province"].ToString();
                    user.PYQuanPin = contact["PYQuanPin"].ToString();
                    user.RemarkName = contact["RemarkName"].ToString();
                    user.RemarkPYQuanPin = contact["RemarkPYQuanPin"].ToString();
                    user.Sex = contact["Sex"].ToString();

                    if (contact["UserName"].ToString().IndexOf("@@") != -1) //群聊
                    {
                        groupList.Add(user);
                        normalList.Add(user);
                    }
                    else
                    {
                        contactList.Add(user); //联系人
                        normalList.Add(user);
                    }
                }
            }
            //上面只是获取多人聊天的名片，这个才是获取多人聊天的具体信息
            BatchGetGroupMembers();
        }

        public List<Object> GetGroupList()
        {
            return groupList;
        }

        public void SendGroupMessage(string groupName, string message)
        {
            SendMsgByUid(message + "  " + DateTime.Now.ToString(), GetGroupIdByName(groupName));
        }

        public string GetGroupIdByName(string groupName)
        {
            foreach (wxContact group in groupList) if (group.ShowName == groupName) return group.UserName;
            return string.Empty;
        }


        /// <summary>
        /// 批量获取所有群聊成员信息
        /// </summary>
        public void BatchGetGroupMembers()
        {
            string r = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString("f0");
            string url = baseUri + "/webwxbatchgetcontact?type=ex&r=" + Common.ConvertDateTimeToInt(DateTime.Now) + "&pass_ticket=" + passTicket;
            string listParam = "[";
            foreach (wxContact group in groupList)
            {
                listParam += ("{\"UserName\":" + group.UserName + ",\"EncryChatRoomId\":\"\"},");
            }
            listParam = listParam.Substring(0, listParam.Length - 1);
            listParam += "]";
            string postParams = "BaseRequest=" + baseRequest2 + "&Count=" + groupList.Count + "&List=" + listParam;

            Models.wxRequestParams.GroupPostParams _GroupPostParams = new Models.wxRequestParams.GroupPostParams();
            Models.wxRequestParams.BaseRequestParam _BaseRequest = new Models.wxRequestParams.BaseRequestParam();
            _BaseRequest.Sid = sid;
            _BaseRequest.Skey = skey;
            _BaseRequest.Uin = uin;
            _BaseRequest.DeviceID = deviceId;
            foreach (wxContact group in groupList)
            {
                Models.wxRequestParams.listParam _listParam = new Models.wxRequestParams.listParam();
                _listParam.UserName = group.UserName;
                _listParam.EncryChatRoomId = "";
                _GroupPostParams.List.Add(_listParam);
            }
            _GroupPostParams.BaseRequest = _BaseRequest;
            _GroupPostParams.Count = groupList.Count;


            JObject result = JsonConvert.DeserializeObject(Http.WebPostWithJsonData(url, JsonConvert.SerializeObject(_GroupPostParams))) as JObject;
            if (result != null)
            {
                foreach (JObject groupInfo in result["ContactList"])
                {
                    Models.wxGroup group = new Models.wxGroup();
                    group.UserName = groupInfo["UserName"].ToString();
                    group.HeadImgUrl = groupInfo["HeadImgUrl"].ToString();
                    group.MemberCount = groupInfo["MemberCount"].ToString();
                    group.EncryChatRoomId = groupInfo["EncryChatRoomId"].ToString();
                    foreach (JObject member in groupInfo["MemberList"])  //完整好友名单
                    {
                        Models.wxGroupMember groupMember = new Models.wxGroupMember();
                        groupMember.UserName = member["UserName"].ToString();
                        groupMember.NickName = member["NickName"].ToString();
                        group.WxGroupMemberList.Add(groupMember);
                    }
                    groupinfoList.Add(group);
                }
            }
        }

        /// <summary>
        /// 获取本次登录会话ID->uuid
        /// </summary>
        /// <returns></returns>
        public bool GetUuid()
        {
            string url = "https://login.weixin.qq.com/jslogin?appid=wx782c26e4c19acffb&fun=new&lang=zh_CN&_=" + Common.ConvertDateTimeToInt(DateTime.Now);
            string ReturnValue = Http.WebGet(url);
            Match match = Regex.Match(ReturnValue, "window.QRLogin.code = (\\d+); window.QRLogin.uuid = \"(\\S+?)\"");
            if (match.Success)
            {
                string code = match.Groups[1].Value;
                uuid = match.Groups[2].Value;
                return code == "200";
            }
            else
                return false;
        }

        /// <summary>
        /// 获取登录二维码
        /// </summary>
        /// <returns></returns>
        public void GenQrCode()
        {
            string url = "https://login.weixin.qq.com/l/" + uuid;
            Image QRCode = Common.GenerateQRCode(url, Color.Black, Color.White);
            if (QRCode != null)
            {
                QRCode.Save("img\\QRcode.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            System.Diagnostics.Process.Start("img\\QRcode.png", "rundll32.exe C://WINDOWS//system32//shimgvw.dll");
        }

        /// <summary>
        /// 登录扫描检测
        /// </summary>
        /// <returns></returns>
        public string Wait4Login()
        {
            //     http comet:
            //tip=1, 等待用户扫描二维码,
            //       201: scaned
            //       408: timeout
            //tip=0, 等待用户确认登录,
            //       200: confirmed
            string tip = "1";
            int try_later_secs = 1;
            int MAX_RETRY_TIMES = 10;
            string code = UNKONWN;
            int retry_time = MAX_RETRY_TIMES;
            string status_code = null;
            string status_data = null;
            while (retry_time > 0)
            {
                string login_result = Http.WebGet("https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?" + "tip=" + tip + "&uuid=" + uuid + "&_=" + Common.ConvertDateTimeToInt(DateTime.Now));
                Match match = Regex.Match(login_result, "window.code=(\\d+)");
                if (match.Success)
                {
                    Trace.WriteLine("微信: 主邏輯 -> 等待檢測 -> 比對成功");
                    status_data = login_result;
                    status_code = match.Groups[1].Value;
                }
                if (status_code == SCANED) //已扫描 未登录
                {
                    Trace.WriteLine("微信: 主邏輯 -> 等待檢測 -> 已掃描 未登入");
                    tip = "0";
                }
                else if (status_code == SUCCESS)  //已扫描 已登录
                {
                    Trace.WriteLine("微信: 主邏輯 -> 等待檢測 -> 已掃描 已登入");
                    match = Regex.Match(status_data, "window.redirect_uri=\"(\\S+?)\"");
                    if (match.Success)
                    {
                        Trace.WriteLine("微信: 主邏輯 -> 等待檢測 -> 已掃描 已登入 -> 比對成功");
                        string _redirectUri = match.Groups[1].Value + "&fun=new";
                        redirectUri = _redirectUri;
                        baseUri = _redirectUri.Substring(0, _redirectUri.LastIndexOf('/'));
                        string temp_host = baseUri.Substring(8);
                        baseHost = temp_host.Substring(0, temp_host.IndexOf('/'));
                        return status_code;
                    }
                }
                else if (status_code == TIMEOUT)  //超时
                {
                    Trace.WriteLine("微信: 主邏輯 -> 等待檢測 -> 超時");
                    tip = "1";
                    retry_time -= 1;
                    Thread.Sleep(try_later_secs * 1000);
                }
                else
                {
                    Trace.WriteLine("微信: 主邏輯 -> 等待檢測 -> NULL");
                    return null;
                }
                Thread.Sleep(800);
            }
            return status_code;
        }

        /// <summary>
        /// 获取skey sid uid pass_ticket  结果存放在cookies中
        /// </summary>
        public bool Login()
        {
            if (redirectUri.Length < 4)
            {
                return false;
            }
            string SessionInfo = Http.WebGet(redirectUri);
            passTicket = SessionInfo.Split(new string[] { "pass_ticket" }, StringSplitOptions.None)[1].TrimStart('>').TrimEnd('<', '/');
            skey = SessionInfo.Split(new string[] { "skey" }, StringSplitOptions.None)[1].TrimStart('>').TrimEnd('<', '/');
            sid = SessionInfo.Split(new string[] { "wxsid" }, StringSplitOptions.None)[1].TrimStart('>').TrimEnd('<', '/');
            uin = SessionInfo.Split(new string[] { "wxuin" }, StringSplitOptions.None)[1].TrimStart('>').TrimEnd('<', '/');
            if (passTicket == "" || skey == "" | sid == "" | uin == "")
            {
                return false;
            }
            baseRequest = "{{\"BaseRequest\":{{\"Uin\":\"{0}\",\"Sid\":\"{1}\",\"Skey\":\"{2}\",\"DeviceID\":\"{3}\"}}}}";
            baseRequest2 = "{{\"Uin\":\"{0}\",\"Sid\":\"{1}\",\"Skey\":\"{2}\",\"DeviceID\":\"{3}\"}}";
            baseRequest = string.Format(baseRequest, uin, sid, skey, deviceId);
            baseRequest2 = string.Format(baseRequest2, uin, sid, skey, deviceId);
            return true;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            string ReturnValue = Http.WebPost(baseUri + "/webwxinit?r=" + Common.ConvertDateTimeToInt(DateTime.Now) + "&lang=en_US" + "&pass_ticket=" + passTicket, baseRequest);
            JObject init_result = JsonConvert.DeserializeObject(ReturnValue) as JObject;
            me = new wxContact();
            me.UserName = init_result["User"]["UserName"].ToString();    
            me.HeadImgUrl = init_result["User"]["HeadImgUrl"].ToString();
            me.NickName = init_result["User"]["NickName"].ToString();
            me.PYQuanPin = init_result["User"]["PYQuanPin"].ToString();
            me.RemarkName = init_result["User"]["RemarkName"].ToString();
            me.RemarkPYQuanPin = init_result["User"]["RemarkPYQuanPin"].ToString();
            me.Sex = init_result["User"]["Sex"].ToString(); 
            foreach (JObject synckey in init_result["SyncKey"]["List"])  //同步键值
            {
                dicSyncKey.Add(synckey["Key"].ToString(), synckey["Val"].ToString());
            }
            foreach (KeyValuePair<string, string> p in dicSyncKey)
            {
                sync_key_str += p.Key + "_" + p.Value + "%7C";
            }
            sync_key_str = sync_key_str.TrimEnd('%', '7', 'C');
            return init_result["BaseResponse"]["Ret"].ToString() == "0";
        }
        #endregion

        public class csMSG
        {
            public int Type { get; set; }
            public string Content { get; set; }
            public string FromUserName { get; set; }
            public string ToUserName { get; set; }
            public string LocalID { get; set; }
            public string ClientMsgId { get; set; }
        }

        public class csBaseRequest
        {
            public string Uin;
            public string Sid;
            public string Skey;
            public string DeviceID;
        }

        public class message
        {
            public csMSG Msg { get; set; }
            public csBaseRequest BaseRequest { get; set; }
        }

        public bool SendMsgByUid(string word, string dst = "filehelper")
        {
            string url = baseUri + "/webwxsendmsg?pass_ticket=" + passTicket;

            message _message = new message();
            csMSG MSG = new csMSG();
            MSG.Type = 1;
            MSG.FromUserName = me.UserName;
            MSG.ToUserName = dst;
            Random rd = new Random();
            double a = rd.NextDouble();
            string para2 = a.ToString("f3").Replace(".", string.Empty);
            string para1 = (DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds.ToString("f0");
            string msg_id = para1 + para2;
            word = Common.ConvertGB2312ToUTF8(word);
            MSG.Content = word;
            MSG.LocalID = msg_id;
            MSG.ClientMsgId = msg_id;
            csBaseRequest BaseRequest = new csBaseRequest();
            BaseRequest.Uin = uin;
            BaseRequest.Sid = sid;
            BaseRequest.Skey = skey;
            BaseRequest.DeviceID = deviceId;

            _message.Msg = MSG;
            _message.BaseRequest = BaseRequest;

            string jsonStr = JsonConvert.SerializeObject(_message);
            string ReturnVal = Http.WebPost2(url, jsonStr);
            JObject jReturn = JsonConvert.DeserializeObject(ReturnVal) as JObject;
            return jReturn["BaseResponse"]["Ret"].ToString() == "0";
        }
    }
}
