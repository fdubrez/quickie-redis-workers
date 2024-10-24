import { createClient } from "npm:redis@^4.5";
import * as prom from "npm:prom-client@^15.1.3";
import express from "npm:express@^4.21.1";

const app = express()

const register = prom.register;
const processTimer = new prom.Summary({
    name: "quickie_summary",
    help: "Time taken worker to process one item",
    labelNames: ["flavor"],
});
app.get('/metrics', async (_, res) => {
	try {
		res.set('Content-Type', register.contentType);
		res.end(await register.metrics());
	} catch (ex) {
		res.status(500).end(ex);
	}
});
const PORT = Deno.env.get('PORT') || 3000;
app.listen(PORT, () => {
    console.log("Hermes HTTP server up and running on port", PORT);
    console.log("Exposing metrics on", "/metrics");
});

const client = createClient({
    url: `redis://${Deno.env.get("REDIS_HOST") || "localhost"}:6379`,
  });
  
await client.connect();
while (true) {
    const {element} = await client.BLPOP("quickie-out", 0);
    const {flavor, elapsed} = JSON.parse(element);
    processTimer.labels({ flavor }).observe(elapsed);
}
