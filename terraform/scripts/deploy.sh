#!/bin/bash

# Install dependencies
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

## docker-compose 
sudo curl -L "https://github.com/docker/compose/releases/download/1.29.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Build local docker image for Weather Service
sudo docker build -t weatherapp /tmp/WeatherService/ -f /tmp/WeatherService/WeatherService/Dockerfile

# Shut down old docker compose (in case of redeploy)
sudo docker-compose -f /tmp/docker/docker-compose.yml down

# Spin up stack with docker compose
sudo docker-compose -f /tmp/docker/docker-compose.yml up -d