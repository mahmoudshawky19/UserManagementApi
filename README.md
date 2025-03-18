
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
     "DefaultConnection": "Server=server;Database=UserManagementDb;Trusted_Connection=True;"
   }
   ```

3. **Set Environment Variables**:
   Ensure that the following environment variables are added to `appsettings.json` or your development environment:

   ```json
   "Jwt": {
     "Key": "secretkey",
     "Issuer": "issuer",
     "Audience": "audience"
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
 ## 3. Unit Testing

This project includes unit tests for critical API functions using `xUnit` and `Moq`.

### **Running Tests**:
To execute unit tests, run the following command in the terminal:
```bash
   dotnet test
```

### **Test Coverage**:
The unit tests cover the following functionalities:
- **Registering a User**
- **Logging In a User**
- **Fetching User Data**
- **Updating a User Profile (Access Control Checks)**

Example unit test for login:
```csharp
[Fact]
public async Task Login_ShouldReturnOk_WhenCredentialsAreCorrect()
{
    var loginDto = new LoginDto { Email = "user@usersimple.com", Password = "Str0ngP@ss2025" };
    var user = new Users { Id = "b4eafc84-a163-4f7f-a464-2f2d04d117e6", UserName = "usersimple", Email = loginDto.Email };

    _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
    _userManagerMock.Setup(um => um.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
    _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
    _signInManagerMock.Setup(sm => sm.SignInAsync(user, false, null)).Returns(Task.CompletedTask);

    var result = await _controller.Login(loginDto);

    Assert.IsType<OkObjectResult>(result);
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

