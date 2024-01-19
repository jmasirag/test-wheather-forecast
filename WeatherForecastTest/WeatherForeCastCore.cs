using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Text;
using System.Text.RegularExpressions;

namespace WeatherForecastTest
{
    public class WeatherForeCastCore
    {
        public ChromeDriver driver;
        private string username;
        private string password;
        public WeatherForeCastCore() {

            Dictionary<string, string> envVariables = LoadEnvironmentVariables("test.env");
            username = envVariables["TEST_USERNAME"];
            password = envVariables["TEST_PASSWORD"];
        }
        protected string ApiKey => GetApiKey();

       
        private string GetApiKey()
        {

            if (!IsLoggedIn())
            {
                Login();
            }

            driver.FindElement(By.Id("user-dropdown")).Click();

            driver.FindElement(By.CssSelector("a[href='/api_keys']")).Click();

            IWebElement table = driver.FindElement(By.ClassName("api-keys"));
            var rows = table.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
            IWebElement row = null;

            foreach (IWebElement element in rows)
            {
                var tds = element.FindElements(By.TagName("td"));
                if (tds != null && tds[2].GetAttribute("innerText").ToLowerInvariant() == "active")
                {
                    row = element;
                    break;
                }

            }

            if (row == null)
                throw new NullReferenceException("cannot find API key!");

            string apiKey = row.FindElement(By.TagName("td")).GetAttribute("innerText");


            return apiKey;

        }

        //TestPass123!
        protected bool IsLoggedIn()
        {
            return driver.FindElements(By.CssSelector("a[href='/users/sign_out']")).Count > 0;
        }

        protected void Login()
        {
            // Use Selenium to navigate to the login page and perform login
            // You may need to modify this based on the actual OpenWeatherMap website structure
            driver.Navigate().GoToUrl("https://home.openweathermap.org/users/sign_in");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("user_email")));
            // Fill in login form
            driver.FindElement(By.Id("user_email")).SendKeys(username);
            driver.FindElement(By.Id("user_password")).SendKeys(password);
            driver.FindElement(By.ClassName("btn-default")).Click();
            // Wait for login to complete (you may use explicit waits)
        }

        protected async Task<WeatherResponseModel> GetWeatherData( string city,string unit="metric")
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://openweathermap.org/data/3.0/weather?q={city}&appid={ApiKey}&units={unit}";

                StringContent content = new StringContent("", Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {

                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WeatherResponseModel>(responseBody);
                }
                else
                {

                    Console.WriteLine($"Error: {response.StatusCode}");
                    throw new Exception("API return invalid Result!");
                }


            }

        }

        protected static double ExtractNumbers(string input)
        {
            // Define the regex pattern for numbers
            string pattern = @"\d+";

            // Use regex to match numbers in the input string
            MatchCollection matches = Regex.Matches(input, pattern);

            // Concatenate matched numbers
            string result = string.Join("", matches);

            return double.Parse(result);
        }



        static Dictionary<string, string> LoadEnvironmentVariables(string filePath)
        {
            Dictionary<string, string> envVariables = new Dictionary<string, string>();

            try
            {

                string[] lines = File.ReadAllLines(filePath);


                foreach (string line in lines)
                {

                    string[] parts = line.Split('=');


                    if (parts.Length == 2)
                    {

                        envVariables[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading environment file: {ex.Message}");
            }

            return envVariables;
        }

        protected void WaitForPageToLoad(IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            // Wait for the document ready state to be complete
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            // Wait for jQuery or JavaScript frameworks to complete
            wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript("return (window.jQuery != null) && (jQuery.active === 0)"));

        }
    }
}
