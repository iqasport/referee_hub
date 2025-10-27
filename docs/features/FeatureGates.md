# Feature Gates

Feature gates allow controlled rollout of new features in the frontend by enabling or disabling them based on user context and URL query parameters.

## Overview

The feature gates system consists of:
- **Backend**: A `FeatureGates` model that defines available feature flags
- **API Endpoint**: `GET /api/v2/users/me/featuregates` returns feature flags for the current user
- **Frontend Hook**: `useFeatureGates()` provides access to feature flags with query parameter overrides

## Backend Configuration

### Adding a New Feature Flag

1. Edit `src/backend/ManagementHub.Models/Domain/User/FeatureGates.cs`
2. Add a new boolean property:

```csharp
public class FeatureGates
{
    public bool IsTestFlag { get; set; }
    public bool IsMyNewFeature { get; set; }  // Add your new flag
}
```

3. The feature flags can be configured per user using the `IContextualOptions<FeatureGates>` system, which is configured in `Program.cs` with the `ConfigureExcos<FeatureGates>("FeatureGates")` call.

### Configuring Feature Flags

Feature flags are configured using the `Excos` contextual options system. This allows different values based on user context. Configuration can be done through:
- Application configuration files (appsettings.json)
- Environment variables
- Custom configuration providers

Example configuration in `appsettings.json`:
```json
{
  "FeatureGates": {
    "IsTestFlag": false,
    "IsMyNewFeature": true
  }
}
```

## Frontend Usage

### Regenerating API Definitions

After adding or modifying feature flags in the backend:

1. Start the backend server:
   ```bash
   cd src/backend/ManagementHub.Service
   dotnet run
   ```

2. In a separate terminal, regenerate the frontend API:
   ```bash
   cd src/frontend
   yarn swaggergen
   ```

This will update `src/frontend/app/store/serviceApi.ts` with the new feature gate definitions.

3. Update the `defaultFeatureGates` object in `src/frontend/app/utils/featureGateUtils.ts`:
   ```typescript
   const defaultFeatureGates: FeatureGates = {
     isTestFlag: false,
     isMyNewFeature: false,  // Add default value for new flag
   };
   ```

This ensures that all feature flags have a boolean default value even when the backend doesn't return data.

### Using the `useFeatureGates` Hook

```tsx
import { useFeatureGates } from '../../utils/featureGateUtils';

function MyComponent() {
  // Destructure only the flags you need
  const { isTestFlag, isMyNewFeature } = useFeatureGates();

  return (
    <div>
      {isTestFlag && <div>Test feature is enabled!</div>}
      {isMyNewFeature && <NewFeatureComponent />}
    </div>
  );
}
```

**Important**: Always use object destructuring to unpack only the specific flags needed in your component. This makes the dependencies clear and improves code readability.

### Query Parameter Overrides

Feature flags can be overridden using the `features` query parameter in the URL:

```
?features=isTestFlag,isMyNewFeature      # Enable both flags
?features=!isTestFlag                     # Disable isTestFlag
?features=isTestFlag,!isMyNewFeature      # Enable isTestFlag, disable isMyNewFeature
```

**Override Rules:**
- Use full property names (e.g., `isTestFlag`) - case insensitive
- Flags prefixed with `!` are set to `false`
- Flags without `!` are set to `true`
- Query parameter values override backend values
- If a flag is not in the query string, the backend value is used
- If the backend doesn't provide a value, it defaults to `false`

### Example

Visit the Admin page with different query parameters:

- Default: `/admin` (uses backend values)
- With test flag enabled: `/admin?features=isTestFlag`
- With test flag disabled: `/admin?features=!isTestFlag`
- Case insensitive: `/admin?features=ISTESTFLAG` or `/admin?features=istestflag`

## Testing

When testing features with feature gates:

1. **Development**: Use query parameters to enable/disable features without code changes
2. **Production**: Feature flags are controlled by backend configuration
3. **User Testing**: Enable features for specific users through contextual configuration

## Best Practices

1. **Naming Convention**: Use descriptive names starting with "Is" (e.g., `IsNewDashboard`, `IsEnhancedSearch`)
2. **Default to False**: New features should default to `false` for safety
3. **Cleanup**: Remove feature flags once features are fully rolled out
4. **Documentation**: Document the purpose of each feature flag
5. **Object Destructuring**: Always destructure to get only the specific flags needed in a component

## Example Implementation

See `src/frontend/app/pages/Admin/Admin.tsx` for a complete example of using feature gates to conditionally render content.
