## Обзор проекта Cafe (Кафе)

Данный проект — это веб-приложение для клиентов и администраторов кафе. Позволяет клиентам делать заказы, отслеживать их статус в реальном времени, добавлять блюда в избранное и т.д. Администраторы же могут добавлять новые блюда, акции, создавать промокоды, просматривать историю заказов.
Система построена на **Clean Architecture** с использованием **C# ASP.NET Core** и **PostgreSQL**.

## Технологический стек

### Backend
**C# 12 / .NET 8** - Основной язык и платформа
**ASP.NET Core MVC** - Веб-фреймворк
**Dapper** - ORM для работы с БД
**PostgreSQL** - Реляционная база данных

### Frontend (встроенный в ASP.NET)
**Razor Pages / MVC Views**
**Статические HTML страницы**
**CSS** - стили для страниц
**JavaScript** - выполнение кода на клиентской стороне

## Начало работы

### Требования
**.NET 8 SDK**
**PostgreSQL 17+**
**Git**

### Установка и запуск
1. Клонируйте репозиторий:
git clone https://github.com/Yaroslav33-v/Cafe.git
cd Cafe
2. В файле CafeWeb/appsettings.json укажите ваши параметры подключения к БД:
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=cafe;Username=postgres;Password=yourpass;Encoding=UTF8"
}

Для использования оплаты также укажите ваши параметры подключения к БД в файле PaymentApi/PaymentApi/appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=bank;Username=postgres;Password=yourpass;Encoding=UTF8"
}

Для телеграм-бота также укажите параметры подключения к БД в файле TelegramBot/TelegramBot/appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=cafe;Username=postgres;Password=yourpass;Encoding=UTF8"
}

Также для телеграм-бота введите токен в файле TelegramBot/TelegramBot/appsettings.json:
"BotConfiguration": {
  "BotToken": "your-token"
}

Получить токен можно в телеграм-боте @BotFather

3. В папке SQL/Cafe найдите скрипт создания таблиц (CREATE.sql) и скрипт вставки данных (INSERT.sql) и запустите их для базы данных кафе (её параметры подключения указывались в CafeWeb/appsettings.json)

4. В папке SQL/BankApi найдите скрипт создания таблиц (CREATE.sql), скрипт создания процедуры оплаты (PaymentProcedure.sql) и скрипт вставки данных (INSERT.sql) и запустите их для базы данных кафе (её параметры подключения указывались в PaymentApi/PaymentApi/appsettings.json)

5. Запустите проект кафе:
cd Cafe/CafeWeb
dotnet run

Для работы оплаты запустите платежный апи:
cd PaymentApi/PaymentApi
dotnet run

Для работы телеграм-бота запустите его:
cd TelegramBot/TelegramBot
dotnet run

6. Откройте в браузере 
https://localhost:7160 или http://localhost:5064

Модуль оплаты никак не связан с настоящими платежными шлюзами, поэтому карты, используемые для оплаты - не существуют в действительности, для оплаты рекомендуется использовать первую карту:

Номер: 4242424242424242
Cvv: 123
ММ/ГГ: 09/26
