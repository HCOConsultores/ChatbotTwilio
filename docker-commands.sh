#!/bin/bash

# Apagar los contenedores
docker-compose down

# Reconstruir las imágenes
docker-compose build

# Levantar los servicios en modo detach
docker-compose up -d
