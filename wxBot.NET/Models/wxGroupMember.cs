namespace WxBotDotNET.Models
{
    public class wxGroupMember:wxContact
    {
        //群成员名
        private string _userName;
        //public new string UserName
        public new string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
            }
        }

        //群成员昵称
        private string _nickName;
        //public new string NickName
        public new string NickName
        {
            get
            {
                return _nickName;
            }
            set
            {
                _nickName = value;
            }
        }
    }
}
