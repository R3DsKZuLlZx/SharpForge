---
slug: "microservices"
title: "Microservices with .NET"
description: "Build distributed systems with microservices architecture. Learn about Docker, Kubernetes, message queues, API gateways, and service-to-service communication patterns."
level: "Advanced"
duration: "6 Hours"
studentCount: "12"
exerciseCount: 15
projectDescription: "Complete e-commerce microservices project"
topics: ["Docker", "Kubernetes", "RabbitMQ", "gRPC"]
learningOutcomes:
  - "Design and architect microservices systems"
  - "Containerize .NET applications with Docker"
  - "Orchestrate containers with Kubernetes"
  - "Implement async messaging with RabbitMQ"
  - "Build high-performance services with gRPC"
  - "Configure API gateways and service discovery"
  - "Implement distributed tracing and logging"
  - "Handle failures with resilience patterns"
prerequisites:
  - "Strong experience with ASP.NET Core Web APIs"
  - "Understanding of REST and HTTP fundamentals"
  - "Familiarity with Entity Framework Core"
  - "Basic knowledge of Docker concepts"
  - "Experience with dependency injection"
  - "Understanding of async/await patterns"
lessons:
  - number: 1
    title: "Introduction to Microservices"
    description: "Understand microservices architecture, benefits, challenges, and when to use it."
    duration: "12 min"
  - number: 2
    title: "Monolith to Microservices"
    description: "Learn strategies for decomposing monolithic applications."
    duration: "12 min"
  - number: 3
    title: "Domain-Driven Design Basics"
    description: "Apply DDD concepts to identify service boundaries."
    duration: "12 min"
  - number: 4
    title: "Docker Fundamentals"
    description: "Learn Docker concepts: images, containers, volumes, and networks."
    duration: "13 min"
  - number: 5
    title: "Dockerizing .NET Applications"
    description: "Create optimized Dockerfiles for .NET applications."
    duration: "13 min"
  - number: 6
    title: "Docker Compose"
    description: "Orchestrate multi-container applications locally."
    duration: "10 min"
  - number: 7
    title: "Synchronous Communication with HTTP"
    description: "Design REST APIs for service-to-service communication."
    duration: "10 min"
  - number: 8
    title: "gRPC in .NET"
    description: "Build high-performance services with Protocol Buffers and gRPC."
    duration: "13 min"
  - number: 9
    title: "Asynchronous Messaging Patterns"
    description: "Understand event-driven architecture and messaging patterns."
    duration: "12 min"
  - number: 10
    title: "RabbitMQ with MassTransit"
    description: "Implement message queues with RabbitMQ and MassTransit."
    duration: "14 min"
  - number: 11
    title: "Event Sourcing and CQRS"
    description: "Apply event sourcing and command query responsibility segregation."
    duration: "13 min"
  - number: 12
    title: "Database per Service Pattern"
    description: "Manage data in distributed systems."
    duration: "10 min"
  - number: 13
    title: "Saga Pattern"
    description: "Handle distributed transactions with sagas."
    duration: "14 min"
  - number: 14
    title: "Outbox Pattern"
    description: "Ensure reliable message publishing with the outbox pattern."
    duration: "10 min"
  - number: 15
    title: "API Gateway Pattern"
    description: "Understand the role of API gateways in microservices."
    duration: "10 min"
  - number: 16
    title: "YARP Reverse Proxy"
    description: "Build API gateways with YARP in .NET."
    duration: "12 min"
  - number: 17
    title: "Authentication and Authorization"
    description: "Implement centralized auth with IdentityServer."
    duration: "14 min"
  - number: 18
    title: "Kubernetes Fundamentals"
    description: "Learn Kubernetes concepts: pods, services, deployments."
    duration: "13 min"
  - number: 19
    title: "Deploying .NET to Kubernetes"
    description: "Create Kubernetes manifests for .NET services."
    duration: "12 min"
  - number: 20
    title: "ConfigMaps and Secrets"
    description: "Manage configuration in Kubernetes."
    duration: "10 min"
  - number: 21
    title: "Health Checks and Probes"
    description: "Implement liveness and readiness probes."
    duration: "10 min"
  - number: 22
    title: "Scaling and Load Balancing"
    description: "Configure horizontal pod autoscaling."
    duration: "12 min"
  - number: 23
    title: "Centralized Logging with Serilog"
    description: "Aggregate logs from multiple services."
    duration: "11 min"
  - number: 24
    title: "Distributed Tracing with OpenTelemetry"
    description: "Trace requests across service boundaries."
    duration: "12 min"
  - number: 25
    title: "Metrics and Monitoring"
    description: "Monitor services with Prometheus and Grafana."
    duration: "12 min"
  - number: 26
    title: "Resilience Patterns"
    description: "Implement retry, circuit breaker, and timeout patterns."
    duration: "12 min"
  - number: 27
    title: "Polly for .NET"
    description: "Use Polly for resilience and transient fault handling."
    duration: "11 min"
  - number: 28
    title: "Building a Complete Microservices System"
    description: "Build an e-commerce system with all patterns applied."
    duration: "14 min"
---
<svg viewBox="0 0 128 128" fill="none" xmlns="http://www.w3.org/2000/svg">
    <circle cx="64" cy="64" r="60" fill="url(#microservices-gradient)"/>
    <circle cx="44" cy="44" r="12" fill="white"/>
    <circle cx="84" cy="44" r="12" fill="white"/>
    <circle cx="44" cy="84" r="12" fill="white"/>
    <circle cx="84" cy="84" r="12" fill="white"/>
    <circle cx="64" cy="64" r="10" fill="white"/>
    <path d="M52 52l8 8M76 52l-8 8M52 76l8-8M76 76l-8-8" stroke="white" stroke-width="3" stroke-linecap="round"/>
    <defs>
        <linearGradient id="microservices-gradient" x1="0" y1="0" x2="128" y2="128" gradientUnits="userSpaceOnUse">
            <stop offset="0" stop-color="#0891b2"/>
            <stop offset="1" stop-color="#0e7490"/>
        </linearGradient>
    </defs>
</svg>

