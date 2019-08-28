namespace EventFeed.Producer.Models
{
    public class HomeModel
    {
        public int ClickCount { get; }

        public HomeModel(int clickCount)
        {
            ClickCount = clickCount;
        }
    }
}
