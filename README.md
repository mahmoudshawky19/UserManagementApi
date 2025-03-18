
# User Management API

This is an API project for user management using ASP.NET Core and JWT for authentication. This application allows you to create, log in, update, delete, and view user data.

## Project Contents:

- **User Account (Account)**:
  - Register and log in using email and password.
  - Manage user personal data.
  - Manage roles (Admin, User).

- **JWT Authentication**:
  - API uses JSON Web Tokens (JWT) for authentication.

---

## 1. Local Setup:

### **Basic Steps:**

1. **Clone the Project**:
   Clone the GitHub repository using the following command:

   ```bash
   git clone https://github.com/mahmoudshawky19/UserManagementApi.git
   ```

2. **Configure the Database**:
   Make sure you've updated the connection string in `appsettings.json` with your own database connection string. You need to have SQL Server set up.

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=yourserver;Database=UserManagementDb;Trusted_Connection=True;"
   }
   ```

3. **Set Environment Variables**:
   Ensure that the following environment variables are added to `appsettings.json` or your development environment:

   ```json
   "Jwt": {
     "Key": "yoursecretkey",
     "Issuer": "yourissuer",
     "Audience": "youraudience"
   }
   ```

   Make sure that `your_secret_key` is strong enough to secure the JWT.

4. **Run Migrations**:
   In the command line, navigate to your project and run migrations to update the database:

   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the Application**:
   To run the application locally:

   ```bash
   dotnet run
   ```

   Once the application is running, the API should be available at `https://localhost:5001` (or the port assigned).

---

## 2. How to Use the API:

### **Register (Register):**
- **Method**: `POST`
- **URL**: `/api/account/register`
- **Payload**:
   ```json
   {
     "Username": "exampleUser",
     "Email": "user@example.com",
     "Password": "Password852"
   }
   ```
- **Expected Response**:
   ```json
   {
     "Message": "User registered successfully"
   }
   ```

### **Login (Login):**
- **Method**: `POST`
- **URL**: `/api/account/login`
- **Payload**:
   ```json
   {
     "Email": "user@example.com",
     "Password": "Password852"
   }
   ```
- **Expected Response**:
   ```json
   {
     "Token": "your_jwt_token"
   }
   ```

### **Get User Data (GetUserById):**
- **Method**: `GET`
- **URL**: `/api/account/{id}`
- **Expected Response**:
   ```json
   {
     "Id": "userId",
     "Username": "exampleUser",
     "Email": "user@example.com",
     "FirstName": "mark",
     "LastName": "Doe",
     "CreatedAt": "2025-03-18T12:00:00Z",
     "UpdatedAt": "2025-03-18T12:00:00Z"
   }
   ```

### **Update User Profile (UpdateUserProfile):**
- **Method**: `PUT`
- **URL**: `/api/account/{id}`
- **Payload**:
   ```json
   {
     "Username": "updatedUser",
     "Email": "updated@example.com",
     "FirstName": "John",
     "LastName": "Doe"
   }
   ```
- **Expected Response**:
   ```json
   {
     "Message": "User profile updated successfully."
   }
   ```
 
---

## 4. Important Links:

- **API Documentation**:  (You can use tools like Swagger to display API documentation)
- **GitHub Repository**:  https://github.com/mahmoudshawky19/UserManagementApi

---
 

### Notes:
- If you need to add or modify environment variables or change API settings, please modify the corresponding files like `appsettings.json` and configuration files.
- Make sure you have the proper tools like `Visual Studio` or `VS Code` for local development.

