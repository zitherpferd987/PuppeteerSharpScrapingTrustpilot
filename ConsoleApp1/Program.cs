using System;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
class Program
{
    static async Task Main()
    {
        Environment.SetEnvironmentVariable("PUPPETEER_DOWNLOAD_HOST", "https://npmmirror.com/mirrors/chromium");

        //await new BrowserFetcher().DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = false, //debug   
            Args = new[] { "--no-sandbox" }
        });

        var page = await browser.NewPageAsync();

        Console.WriteLine("open site...");
        await page.GoToAsync("https://www.trustpilot.com/review/peonytours.com?utm_medium=Trustbox&utm_source=EmailNewsletter3", new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle2 },
            Timeout = 600000
        });

        // mouse roll
        for (int i = 0; i < 10; i++)
        {
            await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight);");
            await Task.Delay(10);
        }

        // 等待评论元素出现
        Console.WriteLine("loading...");
        await page.WaitForSelectorAsync("div[class*='cardWrapper']", new WaitForSelectorOptions
        {
            Timeout = 60000
        });

        // 提取前 10 条评论 HTML
        string reviewHtml = await page.EvaluateFunctionAsync<string>(@"
            () => {
                const cards = document.querySelectorAll('div[class*=""cardWrapper""]');
                return Array.from(cards).slice(0, 10).map(c => c.outerHTML).join('');
            }
        ");

        Console.WriteLine("Loaded");

        // 加载模板
        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Sites\trustpilot-test.cshtml");
        if (!File.Exists(templatePath))
        {
            Console.WriteLine("site error: " + Path.GetFullPath(templatePath));
            return;
        }

        string template = await File.ReadAllTextAsync(templatePath);

        // 替换评论内容
        string resultHtml = template.Replace("{{REVIEWS}}", reviewHtml);

        // 保存最终 HTML
        string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trustpilot-output.html");
        await File.WriteAllTextAsync(outputPath, resultHtml);

        Console.WriteLine($"written：{outputPath}");
        var doc = new HtmlDocument();
        doc.LoadHtml(resultHtml);
        // 所有评论节点（每条评论是一个 <article>）
        var reviewNodes = doc.DocumentNode.SelectNodes("//article[contains(@class, 'styles_reviewCard__')]");

        var reviews = new List<ReviewModel>();

        if (reviewNodes != null)
        {
            foreach (var node in reviewNodes)
            {
                var model = new ReviewModel();

                var reviewerNode = node.SelectSingleNode(".//span[contains(@class, 'styles_consumerName__')]");
                model.Reviewer = reviewerNode?.InnerText.Trim() ?? "";

                var titleNode = node.SelectSingleNode(".//h2[contains(@class, 'typography_heading-xs__')]");
                model.Title = titleNode?.InnerText.Trim() ?? "";

                var contentNode = node.SelectSingleNode(".//p[contains(@class, 'typography_body-l__')]");
                model.Content = contentNode?.InnerText.Trim() ?? "";

                var ratingImg = node.SelectSingleNode(".//img[contains(@alt, 'Rated')]");
                if (ratingImg != null)
                {
                    var alt = ratingImg.GetAttributeValue("alt", "");
                    var match = Regex.Match(alt, @"Rated (\d) out of 5");
                    if (match.Success)
                    {
                        model.Rating = int.Parse(match.Groups[1].Value);
                    }
                }

                reviews.Add(model);
            }
        }

        // 输出
        foreach (var review in reviews)
        {
            Console.WriteLine($"Reviewer: {review.Reviewer}");
            Console.WriteLine($"Title: {review.Title}");
            Console.WriteLine($"Content: {review.Content}");
            Console.WriteLine($"Rating: {review.Rating}");
            Console.WriteLine("------------------------------");
        }

    }
  
    public class ReviewModel
    {
        public string Reviewer { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }


        public ReviewModel()
        {
            // 初始化所有字符串属性
            Reviewer = "";
            Rating = 0;
            Title = "";
            Content = "";
        }
    }
}
