using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Text.RegularExpressions;

namespace WeatherForecastTest
{
    [TestFixture]
    public class TestWeatherUI : WeatherForeCastCore
    {

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
        }
        [TearDown]
        public void TearDown()
        {
            driver.Close();
        }

        [TestCase("Tokyo")]
        [TestCase("Manila")]
        public void TestSearchWeatherFromValidCity(string city)
        {
           IWebElement result =  SearchForWeather(city);
           Assert.True(result.FindElement(By.TagName("h2")).GetAttribute("innerText").ToLowerInvariant().Contains( city.ToLowerInvariant()));

        }

        [TestCase("Tokyo234")]
        [TestCase("Manila234")]
        public void TestSearchWeatherFromInvalidCity(string city)
        {
            IWebElement result = SearchForWeather(city,false);
            Assert.True(result.FindElement(By.ClassName("widget-notification")).GetAttribute("innerText").ToLowerInvariant().Contains("no results for"));

        }

        [TestCase("Tokyo")]
        public void TestTemperatureMatchesFromAPIData(string city)
        {
            IWebElement result = SearchForWeather(city);
            IWebElement currentTemp = result.FindElement(By.ClassName("current-temp"));

            var temperature = currentTemp.FindElement(By.TagName("span")).GetAttribute("innerText");
       
            WeatherResponseModel data = GetWeatherData(city).Result;

            Assert.True(data.main.temp == ExtractNumbers(temperature?? "0"));

        }


        private IWebElement SearchForWeather(string city, bool waitForResult = true)
        {

            if (!IsLoggedIn())
            {
                Login();
            }

            driver.Navigate().GoToUrl("https://openweathermap.org/");
            WaitForPageToLoad(driver);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until((d) =>
            {
                IWebElement element = d.FindElement(By.ClassName("owm-loader-container"));
                string innerHtml = element.GetAttribute("innerHTML").Trim();
                return string.IsNullOrEmpty(innerHtml) || innerHtml == "<!---->";
            });

            IWebElement searchBox = driver.FindElement(By.ClassName("search-block"));
            searchBox.FindElement(By.TagName("input")).SendKeys(city);
            searchBox.FindElement(By.ClassName("button-round")).Click();

            if (waitForResult)
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("search-dropdown-menu")));
                driver.FindElement(By.ClassName("search-dropdown-menu")).FindElement(By.TagName("li")).Click();
            }
            else 
                wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("widget-notification")));
            
            wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("current-container")));
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return driver.FindElement(By.ClassName("current-container"));

        }

    }
}