import pika
import os
import time
import contextlib

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

def main():
    with rabbitmq_connection() as conn:
        channel = conn.channel()

        num_sensors = 4
        for i in range(num_sensors):
            channel.queue_declare(queue=f"temperature_{i}")
            channel.queue_declare(queue=f"humidity_{i}")
            channel.queue_declare(queue=f"pressure_{i}")
            channel.queue_declare(queue=f"wind_speed_{i}")

        generator = WeatherGenerator()
        while True:
            time.sleep(1)
            for i in range(num_sensors):
                temperature = generator.generate_temperature()
                humidity = generator.generate_humidity()
                pressure = generator.generate_pressure()
                wind_speed = generator.generate_wind_speed()
                channel.basic_publish(exchange='', routing_key=f"temperature_{i}", body=str(temperature))
                channel.basic_publish(exchange='', routing_key=f"humidity_{i}", body=str(humidity))
                channel.basic_publish(exchange='', routing_key=f"pressure_{i}", body=str(pressure))
                channel.basic_publish(exchange='', routing_key=f"wind_speed_{i}", body=str(wind_speed))
                print(f"Sent temperature: {temperature}, humidity: {humidity}, pressure: {pressure}, wind speed: {wind_speed}")

if __name__ == "__main__":
    main()
