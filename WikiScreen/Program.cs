using System;
using System.IO;

namespace WikiScreen
{
    public class Program
    {
        static void Main(string[] args)
        {

            var raw_img = ScreenMaker.MakeWikiScreen("http://localhost:9222", "https://pathofexile.gamepedia.com/Carcass_Jack", 2)
                .GetAwaiter().GetResult();
            
            WriteToDisc(raw_img);
        }

        private static void WriteToDisc(string decoded)
        {
            Console.WriteLine(decoded.Length);
            var bytes = Convert.FromBase64String(decoded);
            using (var imageFile = new FileStream("./image.png", FileMode.Create))
            {
                imageFile.Write(bytes ,0, bytes.Length);
                imageFile.Flush();
            }
        }
    }
}