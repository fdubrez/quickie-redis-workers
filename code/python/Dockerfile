ARG PYTHON_VERSION=3.12
FROM python:${PYTHON_VERSION}-alpine AS base

WORKDIR /app
COPY requirements.txt /app/

RUN pip install -r requirements.txt