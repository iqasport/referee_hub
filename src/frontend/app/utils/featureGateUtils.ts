import { useMemo } from 'react';
import { useGetCurrentUserFeatureGatesQuery } from '../store/serviceApi';

/**
 * Hook to get feature gates with query parameter overrides.
 * 
 * Query parameter format: ?features=flag1,!flag2,flag3
 * - flag1: sets flag1 to true
 * - !flag2: sets flag2 to false
 * - flag3: sets flag3 to true
 * 
 * If a feature is not in the query string, the backend value is used.
 * If the backend doesn't provide a value, it defaults to false.
 * 
 * @returns FeatureGates object with flags from backend and query parameter overrides
 */
export function useFeatureGates() {
  const { data: backendGates, isLoading, error } = useGetCurrentUserFeatureGatesQuery();

  const featureGates = useMemo(() => {
    // Start with backend values or empty object
    const gates = { ...backendGates };

    // Parse query parameters
    const params = new URLSearchParams(window.location.search);
    const featuresParam = params.get('features');

    if (featuresParam) {
      // Split by comma and process each feature
      const features = featuresParam.split(',').map(f => f.trim()).filter(f => f);

      features.forEach(feature => {
        if (feature.startsWith('!')) {
          // Feature with ! prefix should be false
          const flagName = feature.substring(1);
          // Convert to property name (e.g., "testFlag" or "isTestFlag")
          const propertyName = getPropertyName(flagName);
          if (propertyName) {
            gates[propertyName] = false;
          }
        } else {
          // Feature without ! should be true
          const propertyName = getPropertyName(feature);
          if (propertyName) {
            gates[propertyName] = true;
          }
        }
      });
    }

    // Ensure all properties default to false if not set
    return gates;
  }, [backendGates]);

  return { featureGates, isLoading, error };
}

/**
 * Convert feature name to property name, handling both camelCase and with/without "is" prefix
 */
function getPropertyName(featureName: string): string | null {
  if (!featureName) return null;

  // Convert to property name with "is" prefix
  // e.g., "testFlag" -> "isTestFlag"
  return 'is' + featureName.charAt(0).toUpperCase() + featureName.slice(1);
}
