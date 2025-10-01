# Chat API - Real-Time Chat Application

A real-time chat application built with ASP.NET Core, SignalR, and PostgreSQL using Clean Architecture.

## Tech Stack

- **Backend**: ASP.NET Core 9.0
- **Real-Time**: SignalR (WebSocket)
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Password Hashing**: BCrypt
- **API Documentation**: Swagger/OpenAPI

## Architecture

Clean Architecture with 4 layers:

```
ChatAPI/
├── ChatAPI.Domain          # Entities (User, Room, Message, UserRoom)
├── ChatAPI.Application     # DTOs, Interfaces, Services
├── ChatAPI.Infrastructure  # DbContext, Repositories, Data Access
└── ChatAPI.API            # Controllers, Hubs, Program.cs
```

### REST API Endpoints

**Auth**
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/users/{id}` - Get user by ID

**Rooms**
- `GET /api/rooms` - Get all rooms
- `GET /api/rooms/{id}` - Get room details with members
- `POST /api/rooms` - Create new room
- `POST /api/rooms/{id}/join` - Join a room
- `POST /api/rooms/{id}/leave` - Leave a room
- `DELETE /api/rooms/{id}` - Delete room (creator only)
- `GET /api/rooms/my-rooms` - Get user's rooms

**Messages**
- `GET /api/rooms/{roomId}/messages` - Get message history (with pagination)
- `GET /api/messages/{id}` - Get specific message

### Real-Time Features (SignalR)

**Hub Methods**
- `JoinRoom(roomId)` - Join a chat room
- `LeaveRoom(roomId)` - Leave a chat room
- `SendMessage(roomId, content)` - Send message to room
- `SendTypingIndicator(roomId, isTyping)` - Send typing status

**Client Events**
- `ReceiveMessage` - Receive new messages
- `UserJoined` - User joined room notification
- `UserLeft` - User left room notification
- `UserTyping` - Typing indicator updates

### Database Schema

**Users**
- Id, Username (unique), Email (unique), PasswordHash, CreatedAt

**Rooms**
- Id, Name, Description, CreatedById, CreatedAt

**Messages**
- Id, Content, UserId, RoomId, SentAt

**UserRooms** (Join table)
- UserId, RoomId, JoinedAt


## TODO

### 1. Logout Functionality
- Implement token revocation mechanism
- Add token blacklist (Redis-based)
- Terminate SignalR connections on logout
- Add logout endpoint to AuthController

### 2. Refresh Token System
- Add RefreshToken entity and repository
- Implement refresh token generation on login
- Create refresh endpoint for token renewal
- Add refresh token rotation for security
- Update token expiration to shorter duration (15 minutes for access token, 7 days for refresh token)

### 3. Scalability & Distributed System
- Add Redis backplane for SignalR multi-server support
- Implement load balancer with sticky sessions
- Add Redis for distributed state management (rate limiting, user presence)
- Deploy to cloud platform (Azure/AWS)
- Configure auto-scaling for high availability
- Add health checks and monitoring

## Authentication
- User registration with email and username
- Login with JWT token generation (24-hour expiration)
- Password hashing with BCrypt
- JWT-based authorization for protected endpoints

## Setup Instructions

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL 16+

### Installation

1. Clone the repository
```bash
git clone <repository-url>
cd chatAPI
```

2. Install PostgreSQL
```bash
# macOS (Homebrew)
brew install postgresql@16
brew services start postgresql@16

# Create database
createdb chatapi
```

3. Update connection string (if needed)
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=chatapi;Username=postgres;Password=postgres"
  }
}
```

4. Apply database migrations
```bash
dotnet ef database update --project ChatAPI.Infrastructure --startup-project ChatAPI.API
```

5. Run the application
```bash
cd ChatAPI.API
dotnet run
```

6. Access Swagger UI
```
http://localhost:5000/swagger
```

## Usage Example

### REST API

1. Register a user
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "alice",
    "email": "alice@example.com",
    "password": "SecurePass123"
  }'
```

2. Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "alice",
    "password": "SecurePass123"
  }'
```

3. Create a room (with token)
```bash
curl -X POST http://localhost:5000/api/rooms \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "General",
    "description": "General discussion"
  }'
```

### SignalR Client (JavaScript)

```javascript
// Install: npm install @microsoft/signalr

import * as signalR from '@microsoft/signalr';

// Create connection
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/chatHub", {
    accessTokenFactory: () => localStorage.getItem("token")
  })
  .build();

// Set up event handlers
connection.on("ReceiveMessage", (message) => {
  console.log(`${message.user.username}: ${message.content}`);
});

connection.on("UserJoined", (data) => {
  console.log(`User ${data.userId} joined`);
});

// Start connection
await connection.start();

// Join a room
await connection.invoke("JoinRoom", "room-id-here");

// Send a message
await connection.invoke("SendMessage", "room-id-here", "Hello everyone!");

// Leave room
await connection.invoke("LeaveRoom", "room-id-here");

// Disconnect
await connection.stop();
```

## Project Structure

```
ChatAPI/
├── ChatAPI.Domain/
│   └── Entities/
│       ├── User.cs
│       ├── Room.cs
│       ├── Message.cs
│       └── UserRoom.cs
│
├── ChatAPI.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
│       ├── AuthService.cs
│       ├── RoomService.cs
│       ├── MessageService.cs
│       └── TokenService.cs
│
├── ChatAPI.Infrastructure/
│   ├── Data/
│   │   └── ChatDbContext.cs
│   ├── Repositories/
│   └── Migrations/
│
└── ChatAPI.API/
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── RoomsController.cs
    │   └── MessagesController.cs
    ├── Hubs/
    │   └── ChatHub.cs
    └── Program.cs
```

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ChatAPI",
    "Audience": "ChatAPIUsers",
    "ExpirationMinutes": 1440
  }
}
```

### CORS
Currently configured to allow all origins in development. Update for production:
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## License

MIT
