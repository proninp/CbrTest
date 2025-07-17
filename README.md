# Currency Gateway API

Веб-API для получения курсов валют от Центрального Банка России с кэшированием и фильтрацией.

## Особенности

- **Получение курсов валют** от SOAP API ЦБ РФ
- **Кэширование в Redis** для повышения производительности
- **Фильтрация** по цифровому (840) или символьному (USD) коду валюты
- **Обработка выходных дней** - автоматический переход к предыдущей пятнице
- **Структурированное логирование** через Serilog
- **Swagger документация** для удобства тестирования

## Технологии

- **ASP.NET Core 3.1** - веб-фреймворк
- **Redis** - кэширование данных
- **Serilog** - структурированное логирование
- **Swagger/OpenAPI** - документация API
- **Newtonsoft.Json** - сериализация JSON
- **Docker** - контейнеризация Redis

## API Endpoints

### Получить курсы валют

```http
GET /api/currency/byNumCode?date=2023-07-25&currencyCode=840
GET /api/currency/byCharCode?date=2023-07-25&currencyCharCode=USD
```

**Параметры:**
- `date` (optional) - дата курса в формате YYYY-MM-DD (по умолчанию - сегодня)
- `currencyCode` (optional) - цифровой код валюты (ISO 4217)
- `currencyCharCode` (optional) - символьный код валюты (USD, EUR, etc.)

**Ответы:**
- `200` - курсы валют успешно получены
- `204` - валюта с указанным кодом не найдена
- `500` - внутренняя ошибка сервера
- `503` - сервис ЦБ РФ временно недоступен

## Старт

### 1. Запустите Redis

```bash
docker-compose up -d
```

### 2. Настройте конфигурацию

Обновите `appsettings.json`:
```json
{
  "RedisOptions": {
    "ConnectionString": "localhost:6379",
    "Password": "root"
  }
}
```

### 3. Запустите приложение

```bash
dotnet restore
dotnet build
dotnet run
```

### 4. Откройте Swagger UI
Перейдите на https://localhost:5001/index.html

## Примеры запросов

```bash
# Все валюты на сегодня
curl "https://localhost:5001/api/currency/byNumCode"

# Доллар США на конкретную дату (по коду)
curl "https://localhost:5001/api/currency/byNumCode?date=2023-07-25&currencyCode=840"

# Евро на конкретную дату (по символу)
curl "https://localhost:5001/api/currency/byCharCode?date=2023-07-25&currencyCharCode=EUR"
```

## Пример ответа

```json
{
  "date": "2023-07-25T00:00:00",
  "currencyRates": [
    {
      "name": "Доллар США",
      "nominal": 1,
      "rate": 91.0671,
      "code": 840,
      "charCode": "USD",
      "unitRate": 91.0671,
      "date": "2023-07-25T00:00:00"
    }
  ]
}
```

## Возможные улучшения

Для асинхронного получения информации я бы добавил Background Service, который периодически (например, каждые 30 минут) обновляет кэш курсов валют в фоне, используя IHostedService для планирования задач и Hangfire или Quartz.NET для надежного выполнения с механизмом ретраев при сбоях подключения к ЦБ РФ.