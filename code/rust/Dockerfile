FROM rust:1.82-alpine as builder
WORKDIR /usr/src/myapp
COPY . .
RUN cargo install --path .

FROM alpine as runner
COPY --from=builder /usr/local/cargo/bin/quickie-rust /usr/local/bin/quickie-rust
CMD ["quickie-rust"]