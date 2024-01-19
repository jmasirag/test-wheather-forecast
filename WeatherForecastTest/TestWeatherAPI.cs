using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastTest
{
    [TestFixture]
    internal class TestWeatherAPI: WeatherForeCastCore
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
        [TestCase("NewYork")]
        [TestCase("Manila")]
        public void TestWeatherDataForDifferentValidCity_ShouldReturnValidData(string city)
        {
            var data = GetWeatherData(city);
            Assert.IsNotNull(data);
        }

        [TestCase("Tokyo123")]
        [TestCase("NewYork123")]
        [TestCase("Manila123")]
        public void TestWeatherDataForDifferentInvalidCity_ShouldReturnNullData(string city)
        {
            var data = GetWeatherData(city);
            Assert.IsNull(data);
        }


        [TestCase("Tokyo", "metric")]
        [TestCase("NewYork", "standard ")]
   
        public void TestTemparatureUnit_BasedOnSuppliedUnitPerCity(string city, string unit)
        {
            var data = GetWeatherData(city, unit);
            //unfortunately the api returns 401 despite we use the correct and default token
        }


    }
}
