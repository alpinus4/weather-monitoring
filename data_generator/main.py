import pika
import os
import time
import contextlib
import datetime
import json

from weather_generator import WeatherGenerator

@contextlib.contextmanager
def rabbitmq_connection():
    rabbitmq_user = os.getenv("RABBITMQ_USER")
    rabbitmq_password = os.getenv("RABBITMQ_PASSWORD")
    rabbitmq_url = os.getenv("RABBITMQ_URL")

    start_time = time.time()
    while time.time() - start_time < 30:
        try:
            credentials = pika.PlainCredentials(rabbitmq_user, rabbitmq_password)
            parameters = pika.ConnectionParameters(rabbitmq_url, credentials=credentials)
            connection = pika.BlockingConnection(parameters)
            break
        except pika.exceptions.AMQPConnectionError as e:
            print(f"Failed to connect to RabbitMQ: {e}")
        time.sleep(1)
    
    yield connection
    connection.close()

def new_sensor_reading(sensor_type, value):
    return {
        "type": sensor_type,
        "value": value,
        "timestamp": datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    }

def main():
    with rabbitmq_connection() as conn:
        channel = conn.channel()

        num_sensors = 4
        sensor_queue = "sensor_data"
        channel.queue_declare(queue=sensor_queue)

        generator = WeatherGenerator()
        while True:
            time.sleep(3)
            for i in range(num_sensors):
                temperature = new_sensor_reading("temperature", generator.generate_temperature())
                humidity = new_sensor_reading("humidity", generator.generate_humidity())
                pressure = new_sensor_reading("pressure", generator.generate_pressure())
                wind_speed = new_sensor_reading("wind_speed", generator.generate_wind_speed())
                channel.basic_publish(exchange='', routing_key=sensor_queue, body=json.dumps(temperature))
                channel.basic_publish(exchange='', routing_key=sensor_queue, body=json.dumps(humidity))
                channel.basic_publish(exchange='', routing_key=sensor_queue, body=json.dumps(pressure))
                channel.basic_publish(exchange='', routing_key=sensor_queue, body=json.dumps(wind_speed))
                print(f"Sent temperature: {temperature}, humidity: {humidity}, pressure: {pressure}, wind speed: {wind_speed}")

if __name__ == "__main__":
    main()
