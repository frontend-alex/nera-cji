# NERA Project Makefile
# Cross-platform build and management commands for NERA project

.PHONY: help build run clean test publish restore watch dev install-deps check-deps

# Default target
help: ## Show this help message
	@echo "==============================================="
	@echo "    NERA - NEXT Event Registration Application"
	@echo "    Project Management Commands"
	@echo "==============================================="
	@echo ""
	@echo "Available Commands:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[32m%-12s\033[0m %s\n", $$1, $$2}'
	@echo ""
	@echo "Usage Examples:"
	@echo "  make build"
	@echo "  make run"
	@echo "  make clean"
	@echo ""

# Project configuration
PROJECT_DIR = nera-cji
CONFIGURATION = Release
VERBOSITY = normal

# Build targets
build: check-deps ## Build the project
	@echo "Building NERA project..."
	@cd $(PROJECT_DIR) && dotnet build --configuration $(CONFIGURATION) --verbosity $(VERBOSITY)
	@echo "Build completed successfully!"

run: check-deps ## Run the project in development mode
	@echo "Starting NERA application..."
	@echo "Application will be available at:"
	@echo "  - HTTPS: https://localhost:5001"
	@echo "  - HTTP:  http://localhost:5000"
	@echo ""
	@echo "Press Ctrl+C to stop the application"
	@cd $(PROJECT_DIR) && dotnet run --configuration $(CONFIGURATION)

dev: check-deps ## Run with hot reload (dotnet watch)
	@echo "Starting NERA with hot reload..."
	@echo "Application will be available at:"
	@echo "  - HTTPS: https://localhost:5001"
	@echo "  - HTTP:  http://localhost:5000"
	@echo ""
	@echo "Hot reload is enabled - changes will be automatically applied"
	@echo "Press Ctrl+C to stop the application"
	@cd $(PROJECT_DIR) && dotnet watch run --configuration $(CONFIGURATION)

watch: dev ## Alias for dev command

# Clean targets
clean: ## Clean build artifacts
	@echo "Cleaning build artifacts..."
	@cd $(PROJECT_DIR) && \
	if [ -d "bin" ]; then rm -rf bin && echo "Removed bin directory"; fi && \
	if [ -d "obj" ]; then rm -rf obj && echo "Removed obj directory"; fi && \
	dotnet nuget locals all --clear && echo "Cleared NuGet cache"
	@echo "Clean completed successfully!"

clean-all: clean ## Clean everything including publish directory
	@echo "Cleaning all artifacts..."
	@if [ -d "publish" ]; then rm -rf publish && echo "Removed publish directory"; fi
	@echo "Complete clean finished!"

# Test targets
test: check-deps ## Run unit tests
	@echo "Running tests..."
	@cd $(PROJECT_DIR) && dotnet test --configuration $(CONFIGURATION) --verbosity $(VERBOSITY)
	@echo "All tests passed!"

test-coverage: check-deps ## Run tests with coverage report
	@echo "Running tests with coverage..."
	@cd $(PROJECT_DIR) && dotnet test --configuration $(CONFIGURATION) --collect:"XPlat Code Coverage" --verbosity $(VERBOSITY)
	@echo "Tests with coverage completed!"

# Package management
restore: check-deps ## Restore NuGet packages
	@echo "Restoring NuGet packages..."
	@cd $(PROJECT_DIR) && dotnet restore --verbosity $(VERBOSITY)
	@echo "Packages restored successfully!"

install-deps: restore ## Alias for restore command

# Publish targets
publish: check-deps ## Publish the project for deployment
	@echo "Publishing NERA for deployment..."
	@if [ -d "publish" ]; then rm -rf publish; fi
	@cd $(PROJECT_DIR) && dotnet publish --configuration $(CONFIGURATION) --output ../publish --self-contained false
	@echo "Publish completed successfully!"
	@echo "Published files are in: publish/"

publish-self-contained: check-deps ## Publish as self-contained deployment
	@echo "Publishing NERA as self-contained..."
	@if [ -d "publish" ]; then rm -rf publish; fi
	@cd $(PROJECT_DIR) && dotnet publish --configuration $(CONFIGURATION) --output ../publish --self-contained true
	@echo "Self-contained publish completed!"
	@echo "Published files are in: publish/"

# Docker targets (if needed in future)
docker-build: ## Build Docker image
	@echo "Building Docker image..."
	@docker build -t nera-cji .
	@echo "Docker image built successfully!"

docker-run: ## Run Docker container
	@echo "Running Docker container..."
	@docker run -p 5000:5000 -p 5001:5001 nera-cji

# Development utilities
check-deps: ## Check if .NET SDK is installed
	@echo "Checking dependencies..."
	@command -v dotnet >/dev/null 2>&1 || { echo ".NET SDK is not installed. Please install .NET 8.0 SDK."; exit 1; }
	@dotnet --version >/dev/null 2>&1 || { echo ".NET SDK is not working properly."; exit 1; }
	@echo ".NET SDK is available: $$(dotnet --version)"

format: check-deps ## Format code using dotnet format
	@echo "Formatting code..."
	@cd $(PROJECT_DIR) && dotnet format
	@echo "Code formatting completed!"

lint: check-deps ## Run code analysis
	@echo "Running code analysis..."
	@cd $(PROJECT_DIR) && dotnet build --configuration $(CONFIGURATION) --verbosity quiet
	@echo "Code analysis completed!"

# Database targets (for future use)
db-migrate: ## Run database migrations
	@echo "Running database migrations..."
	@cd $(PROJECT_DIR) && dotnet ef database update
	@echo "Database migrations completed!"

db-seed: ## Seed database with initial data
	@echo "Seeding database..."
	@cd $(PROJECT_DIR) && dotnet run --configuration $(CONFIGURATION) -- --seed
	@echo "Database seeding completed!"

# Quick development workflow
quick-start: restore build run ## Quick start: restore, build, and run

dev-setup: restore build test ## Development setup: restore, build, and test

# CI/CD targets
ci-build: restore build test ## CI build pipeline
	@echo "CI build pipeline completed successfully!"

ci-publish: ci-build publish ## CI publish pipeline
	@echo "CI publish pipeline completed successfully!"

# All-in-one targets
all: clean restore build test publish ## Do everything: clean, restore, build, test, and publish
	@echo "All tasks completed successfully!"

fresh: clean-all restore build test ## Fresh start: clean everything and rebuild
	@echo "Fresh build completed successfully!"

