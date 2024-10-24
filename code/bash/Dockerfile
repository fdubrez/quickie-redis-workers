FROM redis:7.4-bookworm

RUN apt update && apt install -y jq

ENTRYPOINT [ "/tmp/worker" ]