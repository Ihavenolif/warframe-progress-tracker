FROM python:3.13.7-slim-bookworm

WORKDIR /app

COPY requirements.txt .
COPY *.py .

RUN apt update && apt install -y libpq-dev python3-dev gcc curl

RUN pip install --no-cache-dir -r requirements.txt

CMD ["python3", "server.py"]