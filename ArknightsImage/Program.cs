using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArknightsImage.Properties;
using Newtonsoft.Json.Linq;

namespace ArknightsImage
{
    class Program
    {
        private static void Main(string[] args)
        {
            var data = JObject.Parse(System.Text.Encoding.UTF8.GetString(Resources.character_table));
            var idList = new List<string>();
            foreach (var item in data)
            {
                idList.Add(item.Key);
            }

            //获取alpha通道图片
            const string path = @"D:\解析数据\Texture2D";
            //图片保存位置
            const string savePath = @"D:\解析数据\合成数据";

            if (!Directory.Exists(path))
            {
                Console.WriteLine("素材路径不存在!请检查后重试");
                return;
            }

            var root = new DirectoryInfo(path);
            var files = root.GetFiles();
            //获取所有alpha通道图片 过滤掉npc的 
            var alphaList = files.Select(fileInfo => Path.GetFileName(fileInfo.Name)).Where(x => x.Contains("[alpha].png") && idList.Any(x.Contains)).ToList();
            const int taskNum = 12;
            var taskHandleNum = alphaList.Count % taskNum == 0 ? alphaList.Count / taskNum : alphaList.Count / taskNum + 1;
            var taskList = new List<Task>();
            for (var i = 0; i < taskNum; i++)
            {
                var num = i;
                var task = new Task(() =>
                {
                    var alphas = alphaList.Skip(num * taskHandleNum).Take(taskHandleNum);
                    foreach (var item in alphas)
                    {
                        var rgbFileName = $"{path}\\{item.Replace("[alpha]", string.Empty)}";
                        if (!File.Exists(rgbFileName)) continue;
                        using Bitmap imgRgb = new Bitmap(rgbFileName), imgAlpha = new Bitmap($"{path}\\{item}");
                        //只合成1024*1024的图片
                        if (imgRgb.Width != 1024 || imgRgb.Height != 1024 || imgAlpha.Width != 1024 ||
                            imgAlpha.Height != 1024) continue;
                        //将#号替换为_
                        AlphaBlend(imgRgb, imgAlpha).Save($"{savePath}\\{item.Replace("[alpha]", string.Empty).Replace("#", "_")}", System.Drawing.Imaging.ImageFormat.Png);
                        Console.WriteLine($"{item}导出成功 线程id:{Thread.CurrentThread.ManagedThreadId}");
                    }
                });
                task.Start();
                taskList.Add(task);
            }
            Task.WaitAll(taskList.ToArray());
            Console.WriteLine("处理完成");
        }

        public static Bitmap AlphaBlend(Bitmap rgb, Bitmap alpha)
        {
            for (var x = 0; x < rgb.Width; x++)
            {
                for (var y = 0; y < rgb.Height; y++)
                {
                    var sc = rgb.GetPixel(x, y);
                    var al = alpha.GetPixel(x, y);
                    rgb.SetPixel(x, y, Color.FromArgb(al.B, sc.R, sc.G, sc.B));
                }
            }
            return rgb;
        }
    }
}
