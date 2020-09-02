using System.Configuration;

namespace ArkWorker.Classes
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class SlotData
    {
        public SlotData()
        {
            x = y = r = g = b = 0;
        }

        public bool isVaild => x != 0;

        // position
        public int x { get; set; }
        public int y { get; set; }

        //color
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
    }
}
