
# Convos Application

Convos is a robust communication platform designed to facilitate both structured and unstructured data management using a combination of Microsoft SQL Server and MongoDB. The application is built on ASP.NET Web API and provides two separate backend services to handle relational and NoSQL data efficiently.

## Overview

The Convos application consists of two primary services:

1. **ConvosSQLService**: Manages structured data and leverages Microsoft SQL Server for robust, transactional, and relational data management.
2. **ConvosNoSQLService**: Handles unstructured, flexible data using MongoDB, offering scalable and schema-less data storage.

These services are designed to work seamlessly with the frontend of the Convos platform, which may include features like messaging, real-time communication, and role management.

---

## ConvosSQLService

**ConvosSQLService** is an ASP.NET Web API project built to interact with Microsoft SQL Server. It manages structured data, supports transactions, and offers robust consistency and reliability for critical parts of the Convos application.

### Key Features:

- **Relational Data Management**: Utilizes SQL Server to store structured data like user profiles, roles, and settings.
- **Entity Framework Integration**: Uses Entity Framework Core for database access, supporting migrations and schema updates.
- **Transactional Support**: Ensures data integrity and consistency using SQL Server's ACID-compliant transactions.

### Installation

#### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [SQL Server Management Studio (Optional)](https://aka.ms/ssmsfullsetup)

#### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/ConvosSQLService.git
   cd ConvosSQLService
   ```

2. Update the `appsettings.json` with your SQL Server connection string.
   
3. Apply migrations (if using Entity Framework):
   ```bash
   dotnet ef database update
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Access the API at `https://localhost:5001/swagger/index.html` to see available endpoints.

---

## ConvosNoSQLService

**ConvosNoSQLService** is an ASP.NET Web API project built to interact with MongoDB. It is designed to handle the flexible, schema-less parts of the Convos platform, such as chat history, user activities, and other unstructured data.

### Key Features:

- **Flexible Data Storage**: Uses MongoDB to store unstructured data, making it ideal for handling real-time chat history, files, and user metadata.
- **Scalability**: Leverages MongoDB’s distributed architecture to scale horizontally and handle large volumes of data efficiently.
- **Schema-less Design**: Allows for rapid development and changes to data structures without schema migration.

### Installation

#### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (or MongoDB Atlas for cloud hosting)
- [MongoDB Compass (Optional)](https://www.mongodb.com/products/compass)

#### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/ConvosNoSQLService.git
   cd ConvosNoSQLService
   ```

2. Update the `appsettings.json` with your MongoDB connection string.
   
3. Run the application:
   ```bash
   dotnet run
   ```

4. Access the API at `https://localhost:5001/swagger/index.html` to see available endpoints.

---

## Technology Stack

- **Backend**: ASP.NET Web API, Entity Framework (for SQL Service), MongoDB Driver (for NoSQL Service)
- **Databases**: Microsoft SQL Server (for relational data), MongoDB (for NoSQL data)
- **API Documentation**: Swagger UI available for both services.

---

## Contributing

Contributions are welcome! Feel free to fork the repositories and submit pull requests.

---

### License

MIT License
