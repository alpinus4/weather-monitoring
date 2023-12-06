import time
import random

def strTimeProp(start, end, format, prop):
    stime = time.mktime(time.strptime(start, format))
    etime = time.mktime(time.strptime(end, format))

    ptime = stime + prop * (etime - stime)

    return time.strftime(format, time.localtime(ptime))

def randomDate(start, end, prop):
    return strTimeProp(start, end, '%Y-%m-%d %H:%M:%S', prop)

# Data generator inspired by https://github.com/Renien/generate-weather-data
class WeatherGenerator:
    def __init__(self) -> None:
        self.weather_conditions = {
            "Sunny": {"temperature": (40, 10), "pressure": (1200, 700), "humidity": (70, 55), "wind_speed": (15, 5)},
            "Rain": {"temperature": (25, 15), "pressure": (1200, 700), "humidity": (70, 55), "wind_speed": (25, 10)},
            "Snow": {"temperature": (-1, -7), "pressure": (1200, 700), "humidity": (70, 55), "wind_speed": (10, 2)}
        }
        self.weather = random.choice(list(self.weather_conditions.keys()))
        self.last_weather_change_time = time.time()
    
    def _check_if_weather_should_change(self):
        if self.last_weather_change_time >= 30:
            self.weather = random.choice(list(self.weather_conditions.keys()))
            self.last_weather_change_time = time.time()

    def generate_temperature(self):
        self._check_if_weather_should_change()
        condition = self.weather_conditions[self.weather]
        (tMax, tMin) = condition["temperature"]
        return round(random.uniform(tMax, tMin), 1)
    
    def generate_pressure(self):
        self._check_if_weather_should_change()
        condition = self.weather_conditions[self.weather]
        (pMax, pMin) = condition["pressure"]
        return round(random.uniform(pMax, pMin), 1)
    
    def generate_humidity(self):
        self._check_if_weather_should_change()
        condition = self.weather_conditions[self.weather]
        (hMax, hMin) = condition["humidity"]
        return random.randrange(hMax, hMin, -1)
    
    def generate_wind_speed(self):
        self._check_if_weather_should_change()
        condition = self.weather_conditions[self.weather]
        (wsMax, wsMin) = condition["wind_speed"]
        return round(random.uniform(wsMin, wsMax), 1)