# Email Graph API Worker Service

This repository provides a .NET background worker service that integrates with the Microsoft Graph API to automate the management of unread emails. The service runs continuously in the background, fetching and processing emails to help streamline email management tasks.

## Features

- **Automated Email Processing:** Periodically checks for unread emails and processes them according to defined logic.
- **Microsoft Graph API Integration:** Handles authentication and interactions with Microsoft Graph API for email management.
- **Robust Logging:** Comprehensive logging for tracking the worker’s activities and troubleshooting issues.

## Components

- **Worker Class:**
  - Implements `BackgroundService` for continuous operation.
  - Periodically invokes `APIHandler` to execute email processing tasks.

- **APIHandler Class:**
  - Manages authentication and API interactions with Microsoft Graph.
  - Handles token retrieval, fetching unread emails, and updating email statuses.

## Getting Started

### Prerequisites

- .NET 6.0 or later
- Microsoft Graph API credentials (client ID, client secret, tenant ID)
- An IDE such as Visual Studio or Visual Studio Code

### Installation

1. **Clone the Repository:**

    ```bash
    git clone https://github.com/rathorevishank/EmailGraphAPIWorkerService.git
    cd EmailGraphAPIWorkerService
    ```

2. **Configure Your API Credentials:**

    Open `Secrets.cs` and update it with your Microsoft Graph API credentials.

3. **Build and Run the Project:**

    Restore NuGet packages:

    ```bash
    dotnet restore
    ```

    Build the project:

    ```bash
    dotnet build
    ```

    Run the application:

    ```bash
    dotnet run
    ```

## Configuration

- **API Credentials:** Update `Secrets.cs` with your Microsoft Graph API details (client ID, client secret, tenant ID).
- **Logging:** Configured to output information to the console. Check the logs for details on the worker's operations and any issues encountered.

## Usage

- **Running the Worker Service:** The Worker service starts automatically and performs email processing tasks based on the logic defined in `APIHandler`.
- **Customizing Email Processing:** Modify the `ExecuteLogic` method in `APIHandler` to adjust the email processing behavior according to your needs.

## Contributing

Contributions are welcome! If you have suggestions, improvements, or fixes, please follow these steps:

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Create a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
