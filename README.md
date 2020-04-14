# About TodoList

[![Build Status](https://travis-ci.com/amoraitis/TodoList.svg?branch=develop)](https://travis-ci.com/amoraitis/TodoList)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![codecov](https://codecov.io/gh/amoraitis/TodoList/branch/develop/graph/badge.svg)](https://codecov.io/gh/amoraitis/TodoList)
[![first-timers-only](https://img.shields.io/badge/first--timers--only-friendly-blue.svg?style=flat-square)](https://www.firsttimersonly.com/)

This project is a simple but powerful web application to manage to-dos.

## Features

- Lightweight and fast.
- Supports for two authentication factor.
- Allows you to attach files in to-dos.
- Allows you to specify due date and time in your to-dos.

## Requirements

- A Windows OS
- .NET Core 2.2 or superior
- SQL Server Express 2016 or superior
- A SendGrid account
- (Optional) Google, Facebook, Twitter, Microsoft API keys.

## Set up

1 - Clone this project running `git clone https://github.com/amoraitis/TodoList.git` in your terminal. If you haven't git installed can simply download it and unzip it.

2 - Add your Sendgrid API Key by running `dotnet user-secrets set "SendGrid:ServiceApiKey" "your_secret_key"`.

3 - Run the required infrastructure (PostgreSQL etc) for development purposes
 *  Go to the `TodoList/` folder by running the command `cd TodoList/`
 *  Run the command `docker-compose -f docker-dev-compose.yml up` to bring up the infrastructure. This will setup and run the PostgreSQL server locally.
 *  Note that a folder `pgdata` will be created. PostgreSQL data is persisted in this folder and can be used without having to re-create/seed data over and over during development. Delete this folder to remove any data created during development. 

4 - Go to the `TodoList/TodoList.Web` folder by running the command `cd TodoList/TodoList.Web` or manually navigating into the file system.

5 - Run the command `dotnet tool install -g Microsoft.Web.LibraryManager.Cli` to install Libman.

6 - Run the command `dotnet restore` to install all the dependencies.

7 - Run the command `dotnet build` to compile the project.

8 - Run the command `dotnet run` to start serving the project.

9 - That it's, your application is running in `http://localhost:47818`.

## Support for social login providers

If you want to allow your users to login with their social accounts, e.g. Facebook, follow instructions below.

### Facebook
1 - Follow this [guide](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-2.2#create-the-app-in-facebook) to generate AppID and AppSecret.

2 - Execute the following instructions from `TodoList/TodoList.Web`: 
- `dotnet user-secrets set Authentication:Facebook:AppId <app-id>`
- `dotnet user-secrets set Authentication:Facebook:AppSecret <app-secret>`

### Google
1 - Follow this [guide](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-2.2#create-a-google-api-console-project-and-client-id) to generate ClientID and ClientSecret.

2 - Execute the following instructions from `TodoList/TodoList.Web`: 
- `dotnet user-secrets set Authentication:Google:ClientId <client-id>`
- `dotnet user-secrets set Authentication:Google:ClientSecret <client-secret>`

### Microsoft
1 - Follow this [guide](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins?view=aspnetcore-2.2#create-the-app-in-microsoft-developer-portal) to generate ClientID and ClientSecret.

2 - Execute the following instructions from `TodoList/TodoList.Web`: 
- `dotnet user-secrets set Authentication:Microsoft:ClientId <client-id>`
- `dotnet user-secrets set Authentication:Microsoft:ClientSecret <client-secret>`

### Twitter
1 - Follow this [guide](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/twitter-logins?view=aspnetcore-2.2#create-the-app-in-twitter) to generate App ID and AppSecret.

2 - Execute the following instructions from `TodoList/TodoList.Web`: 
- `dotnet user-secrets set Authentication:Twitter:ConsumerKey <consumer-key>`
- `dotnet user-secrets set Authentication:Twitter:ConsumerSecret <consumer-secret>`

## License

TodoList is open-source software licensed under the [MIT license](LICENSE.txt).
