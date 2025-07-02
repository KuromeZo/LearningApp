# Learning C++ Platform

An AI-powered interactive C++ learning platform that provides personalized coding exercises, intelligent code analysis, and smart hints to help students master C++ programming. (demo product)

## Features

### AI-Powered Learning
- **Intelligent Code Review**: AI analyzes student code and provides detailed feedback
- **Smart Hints**: Personalized hints based on student's current code attempt
- **Exercise Generation**: Automatically generates new C++ exercises using Google Gemini AI
- **Adaptive Scoring**: AI evaluates code quality with scores from 0-100

### Interactive Learning
- **Structured Topics**: Organized learning path through C++ concepts
- **Hands-on Exercises**: Interactive coding exercises with starter code
- **Real-time Feedback**: Instant AI-powered code analysis
- **Progress Tracking**: Monitor learning progress through topics

### Modern Tech Stack
- **Backend**: ASP.NET Core 8.0 Web API
- **Frontend**: Blazor Server with Bootstrap
- **Database**: PostgreSQL (Production) / SQLite (Development)
- **AI Integration**: Google Gemini AI API
- **Deployment**: Docker containers on Railway


** Platform**: [Visit Learning C++ Platform]([https://your-railway-app-url.railway.app](https://learningapp-web-production.up.railway.app/))

## Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│                 │    │                 │    │                 │
│  Blazor Server  │◄──►│   ASP.NET API   │◄──►│   PostgreSQL    │
│   (Frontend)    │    │   (Backend)     │    │   (Database)    │
│                 │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │
         │                       │
         ▼                       ▼
┌─────────────────┐    ┌─────────────────┐
│                 │    │                 │
│   Bootstrap UI  │    │  Google Gemini  │
│     Styling     │    │   AI Service    │
│                 │    │                 │
└─────────────────┘    └─────────────────┘
```

## API Documentation

### Main Endpoints

#### Topics
- `GET /api/topics` - Get all topics
- `GET /api/topics/{id}` - Get specific topic with exercises

#### Exercises
- `GET /api/exercises/{id}` - Get specific exercise
- `POST /api/exercises/{id}/submit` - Submit solution for AI analysis
- `POST /api/exercises/{id}/hint` - Get AI-powered hint
- `POST /api/exercises/generate` - Generate new exercise using AI
- `DELETE /api/exercises/{id}` - Delete exercise
