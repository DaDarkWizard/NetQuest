namespace NetQuest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
            try
            {
                while (true)
                {
                    game.Run();
                }
            }
            catch(Exception)
            {

            }
        }
    }
}