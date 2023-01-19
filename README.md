# Documentation

This demo allows the use of the ILogger and ILoggerProvider interfaces to create a custom logging provider that logs to a Postgresql database.

It integrates with an NET 7 Web API and it's appsettings.json file to log errors to a Postgresql database.

Inside the appsettings.json, there is a Database object which allows you to configure the severity of logging. There are also settings to change the connection string, which fields to include in the log, and the table name where the logs will be stored.

# For Database
Create logerror table in DB
with following fields :
  "LogLevel",
  "ThreadId",
  "EventId",
  "EventName",
  "ExceptionMessage",
  "ExceptionStackTrace",
  "ExceptionSource"

# Open the project in Visual Studio

Open up RoundTheCode.LoggerDb.sln in Visual Studio 2022 and you'll see that the custom logging provider code is in the LoggerDb.Shared project. In appsettings.json, make sure that the connection string is correct.

To run the Web API, you can hit https://localhost:7234. or https://localhost:7234/index.html. To get the exception, you can hit https://localhost:7234/weatherforecast.
