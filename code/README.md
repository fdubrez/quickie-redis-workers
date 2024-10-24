Standalone mode

```shell
# start redis
docker compose up -d

# start exporter
cd exporter && deno run -A main.js

# start a worker
cd deno && deno run -A main.ts

# push some work
for i in {1..1000}; do redis-cli LPUSH quickie '{"file":"'$(echo "FDU" | base64)'"}'; done
```

Compose mode

```shell
# build images
docker build -f bash/Dockerfile -t quickie:bash ./bash
docker build -f python/Dockerfile -t quickie:python ./python
docker build -f rust/Dockerfile -t quickie:rust ./rust
docker build -f dotnetcore/Dockerfile -t quickie:dotnetcore ./dotnetcore

# run with compose
docker compose up -d
```

* Exporter: http://localhost:3000/metrics
* Prometheus: http://localhost:9090/metrics
* Grafana: http://localhost:5000/metrics

## FAQ

Obviously you need:
* redis to have redis-cli command available
* docker
* deno/python/rust/dotnetcore to run worker locally without docker :p 