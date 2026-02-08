# TDP Web Application

A Vue 3 + TypeScript web application for the TDP (Task/Project) management system.

## Features

- User authentication (login/register)
- Project management
- Feature tracking
- Modern responsive UI with Element Plus

## Tech Stack

- **Vue 3** - Progressive JavaScript framework
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool and dev server
- **Vue Router** - Client-side routing
- **Pinia** - State management
- **Element Plus** - Vue 3 UI component library

## Prerequisites

- [Node.js](https://nodejs.org/) (v18 or higher)
- [npm](https://www.npmjs.com/) (comes with Node.js)
- TDP API running on `http://localhost:5201`

## Installation

```bash
# Navigate to the web project
cd src/Tdp.Web

# Install dependencies
npm install
```

## Running the App

### Development

```bash
npm run dev
```

Opens at **http://localhost:5173**

### Production Build

```bash
npm run build
```

Output is in the `dist/` folder.

### Preview Production Build

```bash
npm run preview
```

## Project Structure

```
src/
├── components/     # Reusable Vue components
├── views/          # Page components
├── stores/         # Pinia state stores
├── router/         # Vue Router configuration
├── api/            # API client layer
├── models/         # TypeScript type definitions
└── App.vue         # Root component
```

## Related

- [TDP API](../Tdp.Api/) - Backend API (must be running)
