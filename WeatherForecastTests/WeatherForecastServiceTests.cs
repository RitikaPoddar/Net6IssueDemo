using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using WeatherForecastTests.Models;
using WeatherForecast;
using WeatherForecast.Services;

namespace TestWeatherForecast
{
    public class WeatherForecastServiceTests 
    {
        private Mock<IDemoDBContext> _dbContext;
        private List<Forecast> _forecasts;
        private IWeatherForecastService _service;

        [SetUp]
        public void Setup()
        {
            _dbContext = new Mock<IDemoDBContext>();
            _forecasts = new List<Forecast>
            {
                new Forecast{ TemperatureC = 5, Summary ="Test1", Date = DateTime.Now},
                new Forecast(){ TemperatureC = 10, Summary ="Test2", Date=DateTime.Now}
            };
            _service = new WeatherForecastService(_dbContext.Object);
        }
            /// <summary>
            /// <see cref="WeatherForecastService.GetAllAsync()"/> test.
            /// </summary>
            [Test]
        public async Task GetAllWeatherForecasts_WithData()
        {
            _dbContext.Setup(context => context.weatherForecasts).Returns(DbContextMock.GetDbSet<Forecast>(_forecasts).Object);
            List<Forecast> result = await _service.GetAllAsync();
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo(_forecasts));
        }

    }
}
