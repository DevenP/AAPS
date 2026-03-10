# Global Error Handling Implementation Guide

## Overview

You now have a **complete global error handling system** with 4 layers:

1. **IErrorService** - Centralized error logging & notification
2. **GlobalExceptionHandler Middleware** - Catches unhandled exceptions at the HTTP level
3. **ErrorBoundary Component** - Catches rendering errors in Blazor
4. **Service Error Handling Extensions** - Wraps service calls with try-catch

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│  User Action (Click, Navigate, Submit)                  │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────┐
│  Razor Component                                         │
│  ├─ Wrapped in <ErrorBoundary> (catches render errors) │
│  └─ Injects IErrorService                               │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────┐
│  Service Method (SesiService, ProviderService, etc.)    │
│  └─ Can use errorService.ExecuteSafelyAsync()          │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ▼ (Exception Thrown)
┌─────────────────────────────────────────────────────────┐
│  GlobalExceptionHandler Middleware                      │
│  ├─ Logs exception                                      │
│  ├─ Notifies IErrorService                             │
│  └─ Returns JSON error response                        │
└─────────────────────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────┐
│  ErrorBoundary Component                                │
│  └─ Displays error alert to user                       │
└─────────────────────────────────────────────────────────┘
```

---

## Usage Examples

### **1. Basic Usage in a Component**

```razor
@page "/providers"
@using AAPS.Application.Abstractions.Services
@using AAPS.Application.DTO
@inject IProviderService ProviderService
@inject IErrorService ErrorService

<MudButton OnClick="LoadProviders">Load Providers</MudButton>

@code {
    private async Task LoadProviders()
    {
        try
        {
            var request = new PagedRequest();
            var result = await ProviderService.GetPagedAsync(request);
            // Use result
        }
        catch (Exception ex)
        {
            ErrorService.LogError(ex, "Failed to load providers", "ProviderList");
        }
    }
}
```

### **2. Using the Extension Helper (Cleaner)**

```razor
@page "/providers"
@using AAPS.Application.Abstractions.Services
@using AAPS.Infrastructure.Common.Extensions
@inject IProviderService ProviderService
@inject IErrorService ErrorService

<MudButton OnClick="LoadProviders">Load Providers</MudButton>

@code {
    private async Task LoadProviders()
    {
        await ErrorService.ExecuteSafelyAsync(
            async () =>
            {
                var request = new PagedRequest();
                var result = await ProviderService.GetPagedAsync(request);
                // Use result
            },
            "ProviderList LoadProviders");
    }
}
```

### **3. With Return Value**

```csharp
public async Task LoadData()
{
    var providers = await ErrorService.ExecuteSafelyAsync(
        async () => await ProviderService.GetPagedAsync(new PagedRequest()),
        "Load Providers",
        defaultValue: new PagedResult<ProviderDTO>(new List<ProviderDTO>(), 1, 0, 0));
    
    // providers is guaranteed to be non-null
}
```

### **4. In a Service Method**

```csharp
public class ReportService
{
    private readonly IRepository<Report> _repo;
    private readonly IErrorService _errorService;

    public async Task<Report?> GetReportAsync(int id)
    {
        return await _errorService.ExecuteSafelyAsync(
            async () =>
            {
                var report = await _repo.GetByIdAsync(id);
                if (report == null)
                    throw new KeyNotFoundException($"Report {id} not found");
                
                return report;
            },
            context: $"GetReportAsync({id})",
            defaultValue: null);
    }
}
```

---

## Error Flow Examples

### **Scenario 1: Database Error**

```
1. User clicks "Load Providers" button
2. ProviderService.GetPagedAsync() throws DbException
3. GlobalExceptionHandler catches it
4. ErrorService.LogError() is called
5. ErrorService.OnError event fires
6. ErrorBoundary component displays:
   ┌──────────────────────────────────────┐
   │ ✗ An error occurred processing       │
   │   your request                       │
   │ Context: Request Processing          │
   │                                [X]   │
   └──────────────────────────────────────┘
```

### **Scenario 2: Component Rendering Error**

```
1. ErrorBoundary component detects rendering error
2. Shows error message with "Retry" button
3. User can click retry or close alert
4. Alert disappears when closed or retry succeeds
```

### **Scenario 3: Handled Error**

```
1. User submits invalid data
2. Service throws ArgumentException
3. ErrorService.LogError() is called with custom message
4. User sees friendly message:
   ┌──────────────────────────────────────┐
   │ ✗ Invalid value: Email is required   │
   │ Context: Form Validation              │
   │                                [X]   │
   └──────────────────────────────────────┘
```

---

## Files Created/Modified

### Created
- `AAPS.Application/Abstractions/Services/IErrorService.cs` - Error service interface
- `AAPS.Infrastructure/Services/ErrorService.cs` - Error service implementation
- `AAPS.Infrastructure/Common/Extensions/ServiceErrorHandlingExtensions.cs` - Helper extensions
- `AAPS.Web/Middleware/GlobalExceptionHandler.cs` - Global middleware
- `AAPS.Web/Components/Shared/ErrorBoundary.razor` - Custom error boundary component

### Modified
- `AAPS.Web/Program.cs` - Registered error service and middleware
- `AAPS.Web/Components/Layout/MainLayout.razor` - Wrapped with ErrorBoundary

---

## Best Practices

### ✅ Do's

1. **Always wrap service calls** in try-catch or use ExecuteSafelyAsync
   ```csharp
   await ErrorService.ExecuteSafelyAsync(() => DoSomething(), "context");
   ```

2. **Provide context** for debugging
   ```csharp
   ErrorService.LogError(ex, "Failed to save report", "ReportEdit Save");
   ```

3. **Use extension helpers** for consistency
   ```csharp
   var result = await ErrorService.ExecuteSafelyAsync(
       async () => await _service.DoSomethingAsync(),
       "Operation Name",
       defaultValue: null);
   ```

4. **Handle specific exceptions** for better UX
   ```csharp
   try
   {
       // Save data
   }
   catch (DbUpdateException ex)
   {
       ErrorService.LogError(ex, "Data conflict - please refresh and try again", "Save");
   }
   catch (Exception ex)
   {
       ErrorService.LogError(ex, "Failed to save", "Save");
   }
   ```

### ❌ Don'ts

1. **Don't swallow exceptions silently**
   ```csharp
   // BAD
   try { await DoSomething(); }
   catch { } // Silent failure!
   ```

2. **Don't show raw exception messages to users**
   ```csharp
   // BAD
   catch (Exception ex)
   {
       ErrorService.LogError(ex, ex.Message); // Shows "Sequence contains no elements"
   }
   
   // GOOD
   catch (Exception ex)
   {
       ErrorService.LogError(ex, "No data available"); // User-friendly
   }
   ```

3. **Don't forget to provide context**
   ```csharp
   // BAD
   ErrorService.LogError(ex);
   
   // GOOD
   ErrorService.LogError(ex, "Failed to delete provider", "ProviderEdit Delete");
   ```

---

## Testing Error Handling

### Test Global Exception Handler
```csharp
// In your test, throw an exception from a service
var result = await client.GetAsync("/api/providers");
// Should return 500 with JSON error response
```

### Test Error Boundary
```razor
// Add a component that throws an error
<ErrorBoundary>
    <ThrowErrorComponent /> <!-- Throws when rendering -->
</ErrorBoundary>
// Should show error alert with retry button
```

### Test Service Error Handling
```csharp
[Test]
public async Task ExecuteSafelyAsync_CatchesException()
{
    var errorService = new ErrorService(logger);
    
    var result = await errorService.ExecuteSafelyAsync(
        async () => throw new InvalidOperationException("test"),
        "test",
        defaultValue: "default");
    
    Assert.AreEqual("default", result);
}
```

---

## Logging Configuration

The error service uses `ILogger<ErrorService>`. Configure logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AAPS": "Information",
      "AAPS.Infrastructure.Services.ErrorService": "Warning"
    }
  }
}
```

All errors will be logged with the context and stack trace for debugging.

---

## Summary

You now have:
- ✅ Centralized error logging with `IErrorService`
- ✅ Global exception handling middleware
- ✅ UI error boundary component
- ✅ Helper extensions for safe service calls
- ✅ User-friendly error messages
- ✅ Full error context for debugging

Happy error handling! 🚀
