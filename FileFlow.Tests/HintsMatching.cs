namespace FileFlow.Tests
{
    public class HintsMatcher
    {
        public List<string> items = new();

        public void Add(string item)
        {
            items.Add(item);
        }
        public string First(string input)
        {
            return Sort(input)[0];
        }
        public List<string> Sort(string input)
        {
            return items.OrderByDescending(i => GetSimilarityScore(i, input)).ToList();
        }
        private int GetSimilarityScore(string text, string input)
        {
            string[] searchTerms = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int score = 0;

            foreach (string term in searchTerms)
            {
                if (text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score++;
                }
            }

            return score;
        }
    }
    public class Tests
    {
        private HintsMatcher matcher = new();

        [SetUp]
        public void Setup()
        {
            matcher.Add("MATIC/USDT 2023-07-18 18-01-02.log");
            matcher.Add("MATIC/USDT 2023-07-18 17-18-04.log");
            matcher.Add("MATIC/USDT 2022-07-18 18-01-02.log");
            matcher.Add("MATIC/USDT 2022-07-20 18-01-02.log");

            matcher.Add("ExtensionCheck.txt");
            matcher.Add("ExtensionCheck.png");
            matcher.Add("ExtensionCheck.aaa");
            matcher.Add("ExtensionCheck.aaaa");
            matcher.Add("ExtensionCheck.abaa");
        }

        [Test]
        public void Test1()
        {
            Assert.That(matcher.First("2023-07-18"), Is.EqualTo(matcher.items[0]));
            Assert.That(matcher.First("17-18"), Is.EqualTo(matcher.items[1]));

            Assert.That(matcher.First(".txt"), Is.EqualTo("ExtensionCheck.txt"));
            Assert.That(matcher.First(".png"), Is.EqualTo("ExtensionCheck.png"));
            Assert.That(matcher.First("ng"), Is.EqualTo("ExtensionCheck.png"));
            Assert.That(matcher.First("aaa"), Is.EqualTo("ExtensionCheck.aaa"));
            Assert.That(matcher.First("ba"), Is.EqualTo("ExtensionCheck.abaa"));
        }
    }
}