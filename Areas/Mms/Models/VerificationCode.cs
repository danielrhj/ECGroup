using System;
using System.Drawing;



namespace ECGroup.Models
{
    public class VerificationCode
    {
        private string GenerateCheckCode()
        {
            //创建整型型变量
            int number;
            //创建字符型变量
            char code;
            //创建字符串变量并初始化为空
            string checkCode = String.Empty;
            //创建Random对象
            Random random = new Random();
            //使用For循环生成4个数字
            for (int i = 0; i < 4; i++)
            {
                //生成一个随机数
                number = random.Next();
                //将数字转换成为字符型
                code = (char)('0' + (char)(number % 10));

                checkCode += code.ToString();
            }

            //验证码随机位置插入字母
            string Letter = ((char)(65 + int.Parse(checkCode.Substring(3, 1)))).ToString();
            int intPos = int.Parse(checkCode.Substring(3, 1))%3;

            checkCode = checkCode.Insert(intPos+1, Letter);
            checkCode = checkCode.Replace("I", "X").Replace("O", "Z");

            //将生成的随机数添加到Session中
            System.Web.HttpContext.Current.Session["VerificationCode"] = checkCode;
            //返回字符串
            return checkCode;
        }

        public System.IO.MemoryStream CreateCheckCodeImage()
        {
            string checkCode = GenerateCheckCode();
            //判断字符串不等于空和null
            if (checkCode == null || checkCode.Trim() == String.Empty)
            {
                System.Web.HttpContext.Current.Session["VerificationCode"] = null;
                return null;
            }

            //创建一个位图对象
            System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((checkCode.Length * 15.5)), 22);
            //创建Graphics对象
            Graphics g = Graphics.FromImage(image);

            try
            {
                //生成随机生成器
                Random random = new Random();

                //清空图片背景色
                g.Clear(Color.White);

                //画图片的背景噪音线
                for (int i = 0; i < 2; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);

                    g.DrawLine(new Pen(Color.Black), x1, y1, x2, y2);
                }

                Font font = new System.Drawing.Font("Arial", 12, (System.Drawing.FontStyle.Bold));
                var brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                                                                     Color.Blue, Color.DarkRed, 1.2f, true);
                g.RotateTransform(random.Next(-7,7)); 
                g.DrawString(checkCode, font, brush, 2, 2);

                //画图片的前景噪音点
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);

                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

                //将图片输出到页面上
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms;
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
    }

}