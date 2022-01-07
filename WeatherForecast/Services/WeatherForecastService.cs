using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherForecast.Services
{
    public interface IWeatherForecastService
    {
        Task<Forecast> GetAsync(string summary);
        Task<List<Forecast>> GetAllAsync();
    }
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IDemoDBContext _db;
        public WeatherForecastService(IDemoDBContext context)
        {
            _db = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<Forecast> GetAsync(string summary)
        {
            Forecast result = await _db.weatherForecasts.Where(w => w.Summary == summary).FirstOrDefaultAsync();
            return result;
        }
        public async Task<List<Forecast>> GetAllAsync()
        {
            List<Forecast> result = await _db.weatherForecasts.ToListAsync();
            return result;
        }
    }
}
