# Trustpilot Review Scraper (C# + PuppeteerSharp)

This is a console application built with C# and [PuppeteerSharp](https://github.com/hardkoded/puppeteer-sharp) that automatically scrapes reviews from a specific Trustpilot page. It extracts the top 10 reviews, parses them into structured data, and injects them into an HTML template for display.

---

## ✨ Features

- Uses PuppeteerSharp to launch and control a headless browser
- Scrolls the page to trigger dynamic content loading
- Waits for reviews to fully load on the page
- Extracts structured review data:
  - Reviewer name
  - Review title
  - Review content
  - Star rating (1 to 5)
- Injects review HTML into a Razor-based HTML template
- Outputs a static HTML file containing the reviews
- Logs review details to the console

---

## 📦 Dependencies

- [.NET SDK](https://dotnet.microsoft.com/download) (.NET 6+ recommended)
- NuGet packages:
  - `PuppeteerSharp`
  - `HtmlAgilityPack`

---

## 🚀 How to Use

1. **Clone the Repository**

2. **Install Dependencies**

   Run the following commands in the project directory:

   ```bash
   dotnet add package PuppeteerSharp
   dotnet add package HtmlAgilityPack
