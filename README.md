# Vibe Coding Kafka Demo

This solution demonstrates two .NET microservices communicating indirectly via Apache Kafka while using an in-memory cache for sister city lookups.

## Projects

- `ServiceA`: ASP.NET Core minimal API that accepts city requests, publishes them to Kafka, and exposes an endpoint to poll for sister city responses retrieved from Kafka and stored in an in-memory cache.
- `ServiceB`: .NET Worker service that consumes city requests from Kafka, looks up sister cities using a seeded in-memory cache, and publishes responses back to Kafka for ServiceA to pick up.

## Prerequisites

- .NET SDK 9.0
- Local Kafka broker listening on `localhost:9092` (e.g., via Docker with ZooKeeper and Kafka or Redpanda)

## Running Locally

1. Start Kafka locally (topics `city-requests` and `city-responses` are created automatically on publish/consume).
2. In one terminal, run ServiceB:

   ```bash
   dotnet run --project ServiceB
   ```

3. In another terminal, run ServiceA:

   ```bash
   dotnet run --project ServiceA
   ```

4. Request sister cities for a given city (example uses PowerShell):

   ```powershell
   $response = Invoke-RestMethod -Method Post -Uri https://localhost:5001/cities/request -SkipCertificateCheck -Body (@{ CityName = "Bangkok" } | ConvertTo-Json) -ContentType "application/json"
   $response
   ```

   The response contains a `correlationId` to poll results.

5. Poll for the response using the returned correlation id:

   ```powershell
   Invoke-RestMethod -Method Get -Uri https://localhost:5001/cities/responses/$($response.correlationId) -SkipCertificateCheck
   ```

   When ServiceB finishes processing, the response returns the city and sister cities (example result):

   ```json
   {
     "correlationId": "...",
     "sisterCities": ["Washington D.C.", "Manila", "Beijing", "Hanoi", "Jakarta"]
   }
   ```

## Design Notes

- **In-memory caches**: both services currently use local in-memory data stores (`InMemoryResponseCache` in ServiceA and `CityCache` in ServiceB). These can be replaced with distributed caches (e.g., Redis) later without affecting message contracts.
- **Kafka communication**: ServiceA produces to `city-requests`; ServiceB consumes, enriches data, and publishes to `city-responses`. ServiceA listens via a background Kafka consumer to update its cache.
- **Correlation**: The request API accepts an optional `correlationId` to support client-provided identifiers; otherwise it generates a GUID. Responses reuse this id for lookup.
- **Resilience considerations**: For brevity, the sample omits retries, DLQs, or schema validation—each is a good candidate for future improvement.


