﻿using System;
using System.Text;
using Gma.QrCodeNet.Encoding;
using System.Drawing;

namespace WxBotDotNET
{
    class Common
    {
        //GB2312转化为UTF-8
        public static string ConvertGB2312ToUTF8(string str)
        {
            Encoding utf8;
            Encoding gb2312;
            utf8 = Encoding.GetEncoding("UTF-8");
            gb2312 = Encoding.GetEncoding("GB2312");
            byte[] gb = gb2312.GetBytes(str);
            gb = Encoding.Convert(gb2312, utf8, gb);
            return utf8.GetString(gb);
        }

        //UTF-8转化为GB2312
        public static string ConvertUTF8ToGB2312(string text)
        {
            byte[] bs = Encoding.GetEncoding("UTF-8").GetBytes(text);
            bs = Encoding.Convert(Encoding.GetEncoding("UTF-8"), Encoding.GetEncoding("GB2312"), bs);
            return Encoding.GetEncoding("GB2312").GetString(bs);
        }

        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
        /// <summary>
        /// 将文本转换成二维码
        /// </summary>
        /// <param name="text"></param>
        /// <param name="DarkColor"></param>
        /// <param name="LightColor"></param>
        /// <returns></returns>
        public static Bitmap GenerateQRCode(string text, System.Drawing.Color DarkColor, System.Drawing.Color LightColor)
        {
            QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
            QrCode Code = qrEncoder.Encode(text);
            Bitmap TempBMP = new Bitmap(Code.Matrix.Width, Code.Matrix.Height);
            for (int X = 0; X <= Code.Matrix.Width - 1; X++)
            {
                for (int Y = 0; Y <= Code.Matrix.Height - 1; Y++)
                {
                    if (Code.Matrix.InternalArray[X, Y])
                        TempBMP.SetPixel(X, Y, DarkColor);
                    else
                        TempBMP.SetPixel(X, Y, LightColor);
                }
            }

            return ScaleImage(TempBMP, Code.Matrix.Width * 5, Code.Matrix.Height * 5);
        }

        public static Bitmap ScaleImage(Bitmap pBmp, int pWidth, int pHeight)
        {
            try
            {
                Bitmap tmpBmp = new Bitmap(pWidth, pHeight);
                Graphics tmpG = Graphics.FromImage(tmpBmp);

                tmpG.DrawImage(pBmp,
                    new Rectangle(0, 0, pWidth, pHeight),
                    new Rectangle(0, 0, pBmp.Width, pBmp.Height),
                    GraphicsUnit.Pixel);
                tmpG.Dispose();
                return tmpBmp;
            }
            catch
            {
                return null;
            }
        }
    }
}
