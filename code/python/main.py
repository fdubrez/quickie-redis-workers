from redis import Redis
import os
import json
import hashlib
import base64
from time import process_time_ns


INPUT_QUEUE=os.environ.get('INPUT_QUEUE', "quickie")
OUTPUT_QUEUE=os.environ.get('OUTPUT_QUEUE', "quickie-out")
TIMEOUT=int(os.environ.get('TIMEOUT', 0))
REDIS_HOST=os.environ.get('REDIS_HOST', "localhost")
client = Redis(host=REDIS_HOST, port=6379)

while True:
    (_, element) = client.blpop(INPUT_QUEUE, timeout=TIMEOUT)
    start = process_time_ns()
    event = json.loads(element)
    event['md5'] = hashlib.md5(base64.b64decode(event["file"])).hexdigest()
    event['flavor'] = "python"
    event['elapsed'] = (process_time_ns() - start) / 1e6
    client.lpush(OUTPUT_QUEUE, json.dumps(event))
