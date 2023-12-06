import pika
import os
import time
import contextlib

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
        channel.queue_declare(queue="temperature")
        channel.basic_publish(exchange='',
                      routing_key='hello',
                      body='Hello World!')
        print(" [x] Sent 'Hello World!'")

if __name__ == "__main__":
    main()
