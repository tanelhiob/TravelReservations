# Cosmos Odyssey - Solar System Travel Reservations

A Blazor Server application for booking travel deals across the solar system.

## Features

- **Real-time Pricelist Management**: Automatically fetches and stores the last 15 pricelists from the Cosmos Odyssey API
- **Multi-leg Route Search**: Find optimal routes between any two planets in the solar system
- **Route Filtering & Sorting**: Filter by company and sort by price, distance, or travel time
- **Reservation System**: Make reservations with validation against current valid pricelists
- **Automatic Cleanup**: Old reservations are automatically removed when pricelists expire (keeping only last 15)
- **Expiration Warnings**: Visual indicators when pricelists are about to expire

## How to Run

1. **Prerequisites**: .NET 8.0 SDK

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. **Access the application**: Open your browser to `http://localhost:5000`

## Application Structure

### Data Models
- **PriceList**: Stores pricelist metadata (ID, validation time)
- **Leg**: Travel routes between planets
- **Provider**: Transportation companies with pricing and schedules
- **Reservation**: Customer bookings linked to specific pricelists

### Key Features Implementation

#### Pricelist Management
- Background service checks for expired pricelists every 5 minutes
- Automatically fetches new pricelists when current ones expire
- Maintains exactly 15 most recent pricelists
- Cascading cleanup removes old reservations when pricelists are deleted

#### Reservation Validation
- Prevents bookings on expired pricelists
- Validates all selected routes belong to current valid pricelist
- Shows appropriate error messages for invalid booking attempts

#### Route Search
- Multi-leg journey support with path-finding algorithms
- Only searches within current valid pricelist
- Optimizes routes by selecting cheapest providers for each leg

## API Integration

The application integrates with the Cosmos Odyssey API:
- **Endpoint**: `https://cosmos-odyssey.azurewebsites.net/api/v1.0/TravelPrices`
- **Automatic Updates**: Fetches new pricelists when expired
- **Data Persistence**: Stores complete pricelist data including all legs and providers

## Architecture Highlights

- **In-Memory Database**: Uses Entity Framework with in-memory storage for simplicity
- **Background Services**: Automated pricelist management
- **Blazor Server**: Real-time UI updates with server-side rendering
- **Dependency Injection**: Clean separation of concerns with scoped services