# About TodoList

[![Build Status](https://travis-ci.org/amoraitis/TodoList.svg?branch=develop)](https://travis-ci.org/amoraitis/TodoList)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![codecov](https://codecov.io/gh/amoraitis/TodoList/branch/develop/graph/badge.svg)](https://codecov.io/gh/amoraitis/TodoList)
[![first-timers-only](https://img.shields.io/badge/first--timers--only-friendly-blue.svg?style=flat-square)](https://www.firsttimersonly.com/)

This project is a simple but powerful web application to manage to-dos.

## Requirements

- A Windows OS
- .NET Core 2.1 or superior
- SQL Server Express 2016 or superior
- A SendGrid account

## Set up

1 - Clone this project running `git clone https://github.com/amoraitis/TodoList.git` in your terminal. If you haven't git installed can simply download it and unzip it.

2 - Add your Sendgrid API Key by running `dotnet user-secrets set "SendGrid:ServiceApiKey" "your_secret_key"`.

3 - Go to the `TodoList/Amoraitis.TodoList` folder by running the command `cd TodoList/Amoraitis.TodoList` or manually navigating into the file system.

4 - Run the command `dotnet restore` to install all the dependencies.

5 - Run the command `dotnet build` to compile the project.

6 - Run the command `dotnet run` to start serving the project.

7 - That it's, your application is running in `http://localhost:47818`.

## Features

- Lightweight and fast.
- Supports for two authentication factor.
- Allows you to attach files in to-dos.
- Allows you to specify due date and time in your to-dos.

## License

TodoList is open-source software licensed under the [MIT license](LICENSE.txt).
