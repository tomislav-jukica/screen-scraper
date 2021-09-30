using System;
using System.Collections.Generic;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

namespace ScreenScraper {
    class DataFetcher {
        public void FetchData() {
            Database db = Database.Instance;
            DateTime localTime = DateTime.Now;
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("user-data-dir=C:\\Users\\Stalker\\AppData\\Local\\Google\\Chrome\\User Data\\Default");

            var driver = new ChromeDriver(options)
            {
                Url = "" //Won't work without page URL 
            };
            driver.Navigate();

            int increase = 0;
            bool timeInDays = false;
            bool timeInWeeks = false;
            foreach (IWebElement element in driver.FindElementsByClassName("resource-tile")) {
                string title = element.FindElement(By.XPath(".//h2[@class='resource-tile_info-header_title']")).Text;
                string link = element.FindElement(By.XPath(".//a[@class='resource-tile_link']")).GetAttribute("href");
                string time = element.FindElement(By.XPath(".//span[contains(@class, 'tile-date')]")).Text;
                try {
                    string renpyTag = element.FindElement(By.XPath(".//span[@class='tile-date_mins']")).Text;
                    time = "0";
                } catch { }

                List<string> tags = new List<string>();
                try {
                    string renpyTag = element.FindElement(By.XPath(".//div[@class='pre-renpy']")).Text;
                    tags.Add("Ren''Py");
                } catch{}
                try {
                    string unityTag = element.FindElement(By.XPath(".//div[@class='pre-unity']")).Text;
                    tags.Add("Unity");
                } catch {}


                foreach (IWebElement e in element.FindElements(By.XPath(".//div[contains(@class, 'label--')]"))) {
                    int quote = e.Text.IndexOf("'");
                    string text = e.Text;
                    if(quote > -1) {
                        text = text.Insert(quote, "'");
                    }
                    tags.Add(text);
                }

                int indexQuote = title.IndexOf("'");
                if(indexQuote > -1) {
                    title = title.Insert(indexQuote, "'");
                }


                
                DateTime lastUpdated = new DateTime();

                if (time == "") {
                    timeInDays = true;
                    time = "1";
                }

                if (timeInDays && increase > int.Parse(time)) {
                    timeInWeeks = true;
                    timeInDays = false;
                }

                             

                if (!timeInDays && !timeInWeeks) {
                    lastUpdated = localTime.AddHours(-int.Parse(time));
                }
                if (timeInWeeks) {
                    lastUpdated = localTime.AddDays(-int.Parse(time) * 7);
                } else if (timeInDays) {
                    lastUpdated = localTime.AddDays(-int.Parse(time));
                }
                string dateString = lastUpdated.Date.ToString("dd-MM-yyyy");
                Game game = new Game(title, link, dateString, tags);
                db.InsertGame(game);
                Int32.TryParse(time, out increase);
            }
            driver.Quit();
        }        
    }
}
