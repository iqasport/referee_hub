import { useMemo } from 'react';
import { useGetCurrentUserFeatureGatesQuery } from '../store/serviceApi';
import type { FeatureGates } from '../store/serviceApi';

/**
 * Default values for all feature gates.
 * Update this when adding new feature flags.
 */
const defaultFeatureGates: FeatureGates = {
  isTestFlag: false,
};

/**
 * Hook to get feature gates with query parameter overrides.
 * 
 * Query parameter format: ?features=isTestFlag,!isAnotherFlag
 * - Use full field names (case insensitive)
 * - Prefix with ! to disable a flag
 * 
 * If a feature is not in the query string, the backend value is used.
 * If the backend doesn't provide a value, it defaults to false.
 * 
 * @returns Feature flags object with properties directly accessible
 */
export function useFeatureGates(): FeatureGates {
  const { data: backendGates } = useGetCurrentUserFeatureGatesQuery();

  return useMemo(() => {
    // Start with default values for all known feature gates
    const gates: FeatureGates = { ...defaultFeatureGates };
    
    // Apply backend values if available
    if (backendGates) {
      Object.assign(gates, backendGates);
    }

    // Parse query parameters
    const params = new URLSearchParams(window.location.search);
    const featuresParam = params.get('features');

    if (featuresParam) {
      // Split by comma and process each feature
      const features = featuresParam.split(',').map(f => f.trim()).filter(f => !!f);

      features.forEach(feature => {
        const isNegated = feature.startsWith('!');
        const flagName = isNegated ? feature.substring(1) : feature;
        
        // Find matching property (case insensitive)
        const matchingKey = Object.keys(gates).find(
          key => key.toLowerCase() === flagName.toLowerCase()
        );
        
        if (matchingKey) {
          gates[matchingKey] = !isNegated;
        }
      });
    }

    return gates;
  }, [backendGates]);
}
