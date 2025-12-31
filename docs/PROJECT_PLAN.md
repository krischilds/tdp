# Project Plan

## Goal
Build a two-project solution: (1) a .NET 9 REST API for auth and feature management, (2) a Vue 3 web app using Element Plus and Vite.

## Architecture
- Solution: `Tdp.sln` with `Tdp.Api` (backend) and `Tdp.Web` (frontend).
- Storage: SQLite; data access via Dapper with connection pooling.
- Auth: JWT-based; tokens signed with RSA; public key exposed via JWKS.
- Responses: unified `ResponseDto` wrapper for all API responses.

## Backend (Tdp.Api)
- Endpoints: auth (register, login, refresh, logout), features (CRUD), users (search), user-features (assign/list/remove).
- Tables:
  - `Users`: Id (GUID), Email, PasswordHash, DisplayName, IsActive, CreatedAt, UpdatedAt.
  - `Features`: Id (GUID), Name (unique), Description, CreatedAt.
  - `UserFeatures`: UserId, FeatureId, AssignedAt (many-to-many).
- Behavior:
  - Use JWT for access tokens; refresh tokens stored hashed; device info captured on login/refresh.
  - Return JSON; errors use ProblemDetails; success uses ResponseDto.

## Frontend (Tdp.Web)
- Stack: Vue 3 + Vite + TypeScript; UI library: Element Plus (latest).
- Pages (Phase 1): Login, Register, Dashboard with feature CRUD, user-feature assignment, logout.
- API client: thin wrapper around fetch/axios calling the backend controllers.
- State: Pinia store for auth; persist tokens in localStorage; auto-refresh access token when expired.

## Phase 1 Deliverables
- Working auth (register/login/refresh/logout) end-to-end.
- Feature CRUD UI and user-feature assignment UI when authenticated.
- Protected routes/pages requiring valid access token; graceful handling of expiry via refresh.
- Build scripts via Vite; baseline styling with Element Plus.

## Notes for AI/Automation
- Prefer DTOs with explicit shapes; avoid dynamic typing in API responses.
- Keep JWT secrets/keys configurable via environment; JWKS endpoint must expose the current public key.
- Enforce unique feature names at the DB level; return 409 on conflict.
- Include seed or migration step to create tables if absent.# Project Plan

## Overview
This app will be a dotnet REST API and web application.  It will contain 2 projects in a dotnet solution.

Project 1: backend API project.  
- This will include the c# dotnet code with APIs for authentication, features and userfeatures
- The Auth API will use JWT tokens
- The APIs will access a SQL lite db
- The application will use Dapper.
- There will be a connection pool
- There will be a table called Features.  This will contain a featureId (int or GUID), featureName (string) and featureDescription (string).
- There will be a table called Users that will store user login info. 
- There will be a table called UserFeatures that will relate a user to many features.
- The API will use JWT (JSON Web Tokens) for authentication and returns JSON responses.
- The APIs should return a ResponseDTO so there is a common response.


Project 2: Web app project.  
- We will start with a simple application but add to it in future.
- The initial plan is to build phase 1 of this project. 
- Phase 1 will contain a page (i.e. index.html or cshtml) that is written in VUE 3.
- This will use  Element Plus latest as the UI library.
- For building the project I will use VITE.
- This page will contain a js interface to the API controllers
- This page will allow us to Login, Register (create a user), Check for login.
- Once logged in, this page will allow us to perform CRUD operations on the user features
- Once logged in, the user also has the option to logout.