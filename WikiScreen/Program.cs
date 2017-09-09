namespace WikiScreen
{
    public class Program
    {
        static void Main(string[] args)
        {

            ScreenMaker.MakeWikiScreen("http://localhost:9222", "https://pathofexile.gamepedia.com/Carcass_Jack", 2)
                .GetAwaiter().GetResult();

        }


    }
}