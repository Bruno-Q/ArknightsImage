using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArknightsImage
{
    class Program
    {
        static void Main(string[] args)
        {
            //获取alpha通道图片
            var path = @"D:\明日方舟素材\Texture2D";
            //图片保存位置
            var savePath = @"D:\明日方舟素材\合成数据";
            var root = new DirectoryInfo(path);
            var files = root.GetFiles();
            //获取所有alpha通道图片 过滤掉npc的 
            var alphaList = files.Select(fileInfo => Path.GetFileName(fileInfo.Name)).Where(x => x.Contains("[alpha].png") && !x.Contains("npc") && !x.Contains("build_char")).ToList();

            foreach (var item in alphaList)
            {
                var rgbFileName = $"{path}\\{item.Replace("[alpha]", string.Empty)}";
                if (File.Exists(rgbFileName))
                {
                    using (Bitmap imgrgb = new Bitmap(rgbFileName), imgalpha = new Bitmap($"{path}\\{item}"))
                    {
                        if (imgrgb.Width == 1024 && imgrgb.Height == 1024 && imgalpha.Width == 1024 && imgalpha.Height == 1024)
                        {
                            AlphaBlend(imgrgb, imgalpha).Save($"{savePath}\\{item.Replace("[alpha]", string.Empty)}", System.Drawing.Imaging.ImageFormat.Png);
                            Console.WriteLine($"{item}导出成功");
                        }
                    };
                }
            }
        }

        public static Bitmap AlphaBlend(Bitmap rgb, Bitmap alpha)
        {
            for (int x = 0; x < rgb.Width; x++)
            {
                for (int y = 0; y < rgb.Height; y++)
                {
                    Color sc = rgb.GetPixel(x, y);
                    Color al = alpha.GetPixel(x, y);
                    rgb.SetPixel(x, y, Color.FromArgb(al.B, sc.R, sc.G, sc.B));
                }
            }
            return rgb;
        }
    }
}
