# ConvosSQLService

ConvosSQLService is an ASP.NET Web API project designed to interact with a Microsoft SQL Server database for Convos - Cross Platform Application. 
This service provides endpoints for managing and querying relational data. It's suitable for applications requiring structured data management, transactions, and strong data consistency.

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Local or Azure)
- [SQL Server Management Studio (SSMS)](https://aka.ms/ssmsfullsetup) (Optional for database management)

## Installation

1. **Clone the repository:**

   ```bash
   git clone ...
   cd ConvosSQLService
   ```

2. **Configure SQL Server:**

   - Ensure that SQL Server is running and accessible.
   - Create a new SQL Server database.
   - Update the `appsettings.json` file with your SQL Server connection string:

     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server_name;Database=your_db_name;User Id=your_username;Password=your_password;"
     }
     ```

3. **Apply Entity Framework Migrations:**

   If the project uses Entity Framework for database migrations, run the following commands:

   ```bash
   dotnet ef database update
   ```

4. **Run the application:**

   ```bash
   dotnet run
   ```

   The API should now be running on `https://localhost:5001`.

## API Documentation

Once the service is running, you can access the Swagger documentation at:

```
https://localhost:5001/swagger/index.html
```

This will display all available API endpoints and their corresponding input/output formats.

## Contributing

If you'd like to contribute, feel free to fork the repository and submit pull requests.

---

### License

MIT

# Channel Permission SignalR Notifications

This project demonstrates how to implement and consume SignalR notifications for channel permission changes in a real-time application.

## Backend Implementation

The backend implementation includes:

1. **SignalR Hub Interface**: The `IServerHub` interface defines methods for notifying clients about channel permission changes:
   - `UpdateChannelPermission`: Notifies clients when a specific permission for a channel role changes
   - `OnUpdatePermission`: General permission update notification
   - `UpdateChannel`: Notifies clients when a channel is updated

2. **Channel Service**: The `ChannelService` class handles channel privacy changes and sends notifications:
   - When a channel is set to private/public, it updates the "VIEW_CHANNEL" permission for all roles
   - It sends notifications to all clients in the server about the permission changes

3. **Role Service**: The `RoleService` class handles role permission updates:
   - When updating channel role permissions, it checks if the "VIEW_CHANNEL" permission is being revoked
   - If all roles lose the "VIEW_CHANNEL" permission, it marks the channel as private
   - It sends notifications to all clients in the server about the permission changes

## Frontend Implementation

The frontend implementation includes:

1. **React Component**: The `ChannelPermissionComponent` demonstrates how to:
   - Connect to the SignalR hub
   - Join the server group
   - Listen for permission update events
   - Update the UI based on permission changes

2. **Event Handlers**:
   - `OnUpdatePermission`: Handles general permission updates
   - `UpdateChannelPermission`: Handles specific channel permission updates
   - `UpdateChannel`: Handles channel updates

3. **Permission Management**:
   - Loads initial permissions from the server
   - Maintains a list of visible channels based on permissions
   - Updates the UI when permissions change

## How to Use

1. **Backend Setup**:
   - Ensure your SignalR hub is properly configured
   - Implement the `IServerHub` interface in your hub class
   - Use the `ChannelService` and `RoleService` to manage channel permissions

2. **Frontend Setup**:
   - Install the required dependencies: `npm install`
   - Import the `ChannelPermissionComponent` in your application
   - Pass the required props: `serverId`, `userId`, and `initialChannels`

3. **Testing**:
   - Use the provided test methods to simulate permission changes
   - Verify that the UI updates correctly when permissions change

## Example Usage

```jsx
import React from 'react';
import ChannelPermissionComponent from './ChannelPermissionComponent';

const App = () => {
  const serverId = 'server-123';
  const userId = 'user-456';
  const initialChannels = [
    { id: 'channel-1', name: 'General', type: 'text', isPrivate: false },
    { id: 'channel-2', name: 'Announcements', type: 'text', isPrivate: true },
    { id: 'channel-3', name: 'Random', type: 'text', isPrivate: false }
  ];

  return (
    <div className="app">
      <h1>Server Channels</h1>
      <ChannelPermissionComponent
        serverId={serverId}
        userId={userId}
        initialChannels={initialChannels}
      />
    </div>
  );
};

export default App;
```

## SignalR Events

The following SignalR events are used for channel permission notifications:

1. **OnUpdatePermission**:
   - Triggered when a role permission is updated
   - Parameters: `serverId`, `roleId`, `permissionCode`

2. **UpdateChannelPermission**:
   - Triggered when a specific channel permission is updated
   - Parameters: `serverId`, `channelId`, `roleId`, `permissionCode`, `isGranted`

3. **UpdateChannel**:
   - Triggered when a channel is updated
   - Parameters: `serverId`, `channelName`

## Best Practices

1. **Permission Checking**:
   - Always check permissions on the server side
   - Use the SignalR notifications to update the UI, not to determine access

2. **Error Handling**:
   - Implement proper error handling for SignalR connections
   - Provide fallback mechanisms when the connection is lost

3. **Performance**:
   - Minimize the number of permission checks
   - Use efficient data structures to track visible channels

4. **Security**:
   - Never trust client-side permission checks
   - Always verify permissions on the server side

# SignalR Channel Permissions Demo

This is a React application that demonstrates how to handle channel permission changes with SignalR. The application allows you to:

1. View a list of channels
2. Add new channels
3. Remove channels
4. See real-time updates when channel permissions change

## Prerequisites

- Node.js (v14 or later)
- npm or yarn

## Getting Started

1. Clone the repository
2. Install dependencies:
   ```
   npm install
   ```
   or
   ```
   yarn install
   ```

3. Start the development server:
   ```
   npm start
   ```
   or
   ```
   yarn start
   ```

4. Open your browser and navigate to `http://localhost:3000`

## How It Works

The application connects to a SignalR hub and listens for channel permission changes. When a channel's permissions change, the application updates the UI in real-time.

### Key Components

- `App.tsx`: The main application component
- `App.css`: Styles for the application
- `signalrService.ts`: Service for connecting to the SignalR hub

## Backend Requirements

The application expects a SignalR hub with the following methods:

- `GetChannels`: Returns a list of channels
- `AddChannel`: Adds a new channel
- `RemoveChannel`: Removes a channel

The hub should also send notifications when channel permissions change.

## License

MIT
