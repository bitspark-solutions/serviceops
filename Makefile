# Makefile for ServiceOps Docker environment

# Variables
COMPOSE_FILE = docker-compose.dev.yaml
PROD_COMPOSE_FILE = docker-compose.prod.yml
SERVICE_NAME = app

.PHONY: help build up down logs ps restart rebuild shell clean prune dev prod

help:
	@echo "Usage: make [command]"
	@echo ""
	@echo "Commands:"
	@echo "  dev            Start development environment (alias for up)"
	@echo "  prod           Start production environment (alias for up-prod)"
	@echo "  build          Build or rebuild services"
	@echo "  up             Create and start containers in detached mode"
	@echo "  down           Stop and remove containers, networks, images, and volumes"
	@echo "  logs           Follow log output for the application service"
	@echo "  ps             List containers"
	@echo "  restart        Restart services"
	@echo "  rebuild        Rebuild the application service and restart all services"
	@echo "  shell          Access the application container's shell"
	@echo "  clean          Remove stopped containers and dangling images"
	@echo "  prune          Remove all unused local volumes, networks, and images (use with caution)"
	@echo ""
	@echo "Production specific commands (assuming docker-compose.prod.yml exists):"
	@echo "  build-prod     Build or rebuild services for production"
	@echo "  up-prod        Create and start production containers in detached mode"
	@echo "  down-prod      Stop and remove production containers, networks, images, and volumes"
	@echo "  logs-prod      Follow log output for the production application service"

# Development commands
build:
	docker-compose -f $(COMPOSE_FILE) build

up:
	docker-compose -f $(COMPOSE_FILE) up -d

down:
	docker-compose -f $(COMPOSE_FILE) down --remove-orphans

logs:
	docker-compose -f $(COMPOSE_FILE) logs -f $(SERVICE_NAME)

ps:
	docker-compose -f $(COMPOSE_FILE) ps

restart:
	docker-compose -f $(COMPOSE_FILE) restart

rebuild:
	docker-compose -f $(COMPOSE_FILE) build --no-cache $(SERVICE_NAME)
	docker-compose -f $(COMPOSE_FILE) up -d --force-recreate $(SERVICE_NAME)

# Access the app container shell (assuming bash is available)
shell:
	docker-compose -f $(COMPOSE_FILE) exec $(SERVICE_NAME) /bin/bash

# Clean up Docker resources
clean:
	docker ps -aq -f status=exited | xargs -r docker rm
	docker images -q -f dangling=true | xargs -r docker rmi

prune:
	docker system prune -af --volumes

dev:
	@echo "Starting development environment..."
	@make up

prod:
	@echo "Starting production environment..."
	@make up-prod

# Production commands
build-prod:
	@echo "Building production images with optimized settings..."
	docker-compose -f $(COMPOSE_FILE) -f $(PROD_COMPOSE_FILE) build --no-cache

up-prod:
	@echo "Starting production environment with restart policies..."
	docker-compose -f $(COMPOSE_FILE) -f $(PROD_COMPOSE_FILE) up -d

down-prod:
	@echo "Stopping production environment and cleaning up..."
	docker-compose -f $(COMPOSE_FILE) -f $(PROD_COMPOSE_FILE) down --remove-orphans --volumes

logs-prod:
	docker-compose -f $(COMPOSE_FILE) -f $(PROD_COMPOSE_FILE) logs -f $(SERVICE_NAME)

restart-prod:
	@echo "Restarting production containers..."
	docker-compose -f $(COMPOSE_FILE) -f $(PROD_COMPOSE_FILE) restart

rebuild-prod:
	@echo "Rebuilding production service..."
	docker-compose -f $(COMPOSE_FILE) -f $(PROD_COMPOSE_FILE) build --no-cache $(SERVICE_NAME)
	docker-compose -f $(COMPOSE_FILE) -f $(PROD_COMPOSE_FILE) up -d --force-recreate $(SERVICE_NAME)

shell-prod:
	@echo "Accessing production container shell..."
	docker-compose -f $(COMPOSE_FILE) -f $(PROD_COMPOSE_FILE) exec $(SERVICE_NAME) /bin/bash