import pika
import os
import time
import contextlib
import datetime
import json
import sys

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

def new_sensor_reading(sensor_id, sensor_type, value):
    return {
        "sensor_id": sensor_id,
        "type": sensor_type,
        "value": value,
        "timestamp": (datetime.datetime.now()+ datetime.timedelta(hours=1)).strftime("%Y-%m-%dT%H:%M:%S"),
    }

def main():
    with rabbitmq_connection() as conn:
        channel = conn.channel()

        num_sensors = 4
        sensor_queue = "sensor_data"
        channel.queue_declare(queue=sensor_queue)

        generator = WeatherGenerator()
        try:
            while True:
                time.sleep(3)
                for i in range(num_sensors):
                    temperature = new_sensor_reading(i, "temperature", generator.generate_temperature())
                    humidity = new_sensor_reading(i, "humidity", generator.generate_humidity())
                    pressure = new_sensor_reading(i, "pressure", generator.generate_pressure())
                    wind_speed = new_sensor_reading(i, "wind_speed", generator.generate_wind_speed())
                    channel.basic_publish(exchange='', routing_key=sensor_queue, body=json.dumps(temperature))
                    channel.basic_publish(exchange='', routing_key=sensor_queue, body=json.dumps(humidity))
                    channel.basic_publish(exchange='', routing_key=sensor_queue, body=json.dumps(pressure))
                    channel.basic_publish(exchange='', routing_key=sensor_queue, body=json.dumps(wind_speed))
                    print(f"Sent {temperature}, {humidity}, {pressure}, {wind_speed}")
        except KeyboardInterrupt:
            print("Interrupted by user, exiting...")
            sys.exit(0)

if __name__ == "__main__":
    main()
