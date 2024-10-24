import { createClient } from "npm:redis@^4.5";
import {decodeBase64} from "jsr:@std/encoding/base64";
import { crypto } from "jsr:@std/crypto";
import { encodeHex } from "jsr:@std/encoding/hex";


const INPUT_QUEUE = Deno.env.get("INPUT_QUEUE") || "quickie";
const TIMEOUT = Deno.env.get("TIMEOUT") || 0;
const OUTPUT_QUEUE = Deno.env.get("OUTPUT_QUEUE") || "quickie-out";
const REDIS_HOST = Deno.env.get("REDIS_HOST") || "localhost";

const client = createClient({
    url: `redis://${REDIS_HOST}:6379`,
  });
  
await client.connect();
while (true) {
    const {element} = await client.BLPOP(INPUT_QUEUE, TIMEOUT);
    const t0 = performance.now();
    const event = JSON.parse(element);
    event.md5 = encodeHex(await crypto.subtle.digest("MD5", decodeBase64(event.file)));
    event.elapsed = performance.now() - t0;
    event.flavor = "deno";
    await client.LPUSH(OUTPUT_QUEUE, JSON.stringify(event));
}
