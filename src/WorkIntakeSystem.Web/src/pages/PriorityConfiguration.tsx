import React, { useState, useEffect } from 'react';
import {
  PriorityConfiguration as PriorityConfigurationType,
  PriorityAlgorithmConfig,
  TimeDecayConfig,
  BusinessValueWeightConfig,
  CapacityAdjustmentConfig,
  BusinessVertical,
  PriorityPreviewResult,
  PriorityTrendAnalysis,
  PriorityEffectivenessMetric,
  PriorityRecommendation,
  PriorityConfigValidationResult
} from '../types';
import { apiService } from '../services/api';

interface PriorityConfigurationProps {
  businessVerticalId?: number;
}

const PriorityConfiguration: React.FC<PriorityConfigurationProps> = ({ businessVerticalId }) => {
  // Core state
  const [activeTab, setActiveTab] = useState(0);
  const [selectedBusinessVertical, setSelectedBusinessVertical] = useState<number>(businessVerticalId || 0);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);

  // Data state
  const [businessVerticals, setBusinessVerticals] = useState<BusinessVertical[]>([]);
  const [algorithmConfig, setAlgorithmConfig] = useState<PriorityAlgorithmConfig>({
    algorithmType: 'Enhanced',
    baseWeight: 1.0,
    timeDecayWeight: 1.0,
    businessValueWeight: 1.0,
    capacityAdjustmentWeight: 1.0,
    customWeights: {},
    isActive: true,
    lastModified: new Date().toISOString(),
    modifiedBy: ''
  });
  const [timeDecayConfig, setTimeDecayConfig] = useState<TimeDecayConfig>({
    isEnabled: true,
    maxMultiplier: 2.0,
    decayRate: 0.01,
    startDelayDays: 7,
    decayFunction: 'Logarithmic',
    functionParameters: {}
  });
  const [businessValueConfig, setBusinessValueConfig] = useState<BusinessValueWeightConfig>({
    baseMultiplier: 1.0,
    categoryWeights: {},
    verticalWeights: {},
    strategicAlignmentMultiplier: 1.2,
    roiThreshold: 0.15,
    roiBonusMultiplier: 1.5
  });
  const [capacityConfig, setCapacityConfig] = useState<CapacityAdjustmentConfig>({
    isEnabled: true,
    maxAdjustmentFactor: 1.5,
    minAdjustmentFactor: 0.5,
    optimalUtilizationPercentage: 80.0,
    adjustmentCurve: 'Linear',
    departmentSpecificFactors: {}
  });

  // Analytics state
  const [previewResult, setPreviewResult] = useState<PriorityPreviewResult | null>(null);
  const [trendAnalysis, setTrendAnalysis] = useState<PriorityTrendAnalysis | null>(null);
  const [effectivenessMetrics, setEffectivenessMetrics] = useState<PriorityEffectivenessMetric[]>([]);
  const [recommendations, setRecommendations] = useState<PriorityRecommendation | null>(null);
  const [validationResult, setValidationResult] = useState<PriorityConfigValidationResult | null>(null);

  // Load initial data
  useEffect(() => {
    loadInitialData();
  }, [selectedBusinessVertical]);

  const loadInitialData = async () => {
    try {
      setLoading(true);
      
      // Load business verticals
      const verticalsResponse = await apiService.get('/api/business-verticals');
      setBusinessVerticals(verticalsResponse.data);

      if (selectedBusinessVertical > 0) {
        // Load configurations for selected business vertical
        await Promise.all([
          loadConfigurations(),
          loadAlgorithmConfig(),
          loadTimeDecayConfig(),
          loadBusinessValueConfig(),
          loadCapacityConfig(),
          loadAnalytics()
        ]);
      }
    } catch (error) {
      console.error('Error loading initial data:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadConfigurations = async () => {
    if (selectedBusinessVertical === 0) return;
    
    try {
      await apiService.get(`/api/priorityconfiguration/business-vertical/${selectedBusinessVertical}`);
      // setConfigurations(response.data);
    } catch (error) {
      console.error('Error loading configurations:', error);
    }
  };

  const loadAlgorithmConfig = async () => {
    if (selectedBusinessVertical === 0) return;
    
    try {
      const response = await apiService.get(`/api/priorityconfiguration/algorithm/${selectedBusinessVertical}`);
      setAlgorithmConfig(response.data);
    } catch (error) {
      console.error('Error loading algorithm config:', error);
    }
  };

  const loadTimeDecayConfig = async () => {
    if (selectedBusinessVertical === 0) return;
    
    try {
      const response = await apiService.get(`/api/priorityconfiguration/time-decay/${selectedBusinessVertical}`);
      setTimeDecayConfig(response.data);
    } catch (error) {
      console.error('Error loading time decay config:', error);
    }
  };

  const loadBusinessValueConfig = async () => {
    if (selectedBusinessVertical === 0) return;
    
    try {
      const response = await apiService.get(`/api/priorityconfiguration/business-value-weights/${selectedBusinessVertical}`);
      setBusinessValueConfig(response.data);
    } catch (error) {
      console.error('Error loading business value config:', error);
    }
  };

  const loadCapacityConfig = async () => {
    if (selectedBusinessVertical === 0) return;
    
    try {
      const response = await apiService.get(`/api/priorityconfiguration/capacity-adjustment/${selectedBusinessVertical}`);
      setCapacityConfig(response.data);
    } catch (error) {
      console.error('Error loading capacity config:', error);
    }
  };

  const loadAnalytics = async () => {
    if (selectedBusinessVertical === 0) return;
    
    try {
      const fromDate = new Date();
      fromDate.setMonth(fromDate.getMonth() - 3);
      const toDate = new Date();

      const [trendsResponse, metricsResponse, recommendationsResponse] = await Promise.all([
        apiService.get(`/api/priorityconfiguration/analytics/trends/${selectedBusinessVertical}?fromDate=${fromDate.toISOString()}&toDate=${toDate.toISOString()}`),
        apiService.get(`/api/priorityconfiguration/analytics/effectiveness/${selectedBusinessVertical}`),
        apiService.get(`/api/priorityconfiguration/analytics/recommendations/${selectedBusinessVertical}`)
      ]);

      setTrendAnalysis(trendsResponse.data);
      setEffectivenessMetrics(metricsResponse.data);
      setRecommendations(recommendationsResponse.data);
    } catch (error) {
      console.error('Error loading analytics:', error);
    }
  };

  const saveConfiguration = async () => {
    if (selectedBusinessVertical === 0 || !hasUnsavedChanges) return;

    try {
      setSaving(true);

      // Save all configurations
      await Promise.all([
        apiService.post(`/api/priorityconfiguration/algorithm/${selectedBusinessVertical}`, algorithmConfig),
        apiService.post(`/api/priorityconfiguration/time-decay/${selectedBusinessVertical}`, timeDecayConfig),
        apiService.post(`/api/priorityconfiguration/business-value-weights/${selectedBusinessVertical}`, businessValueConfig),
        apiService.post(`/api/priorityconfiguration/capacity-adjustment/${selectedBusinessVertical}`, capacityConfig)
      ]);

      setHasUnsavedChanges(false);
      alert('Configuration saved successfully!');
      
      // Refresh data
      await loadInitialData();
    } catch (error) {
      console.error('Error saving configuration:', error);
      alert('Error saving configuration. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const previewChanges = async () => {
    if (selectedBusinessVertical === 0) return;

    try {
      const response = await apiService.post(`/api/priorityconfiguration/preview/${selectedBusinessVertical}`, algorithmConfig);
      setPreviewResult(response.data);
    } catch (error) {
      console.error('Error previewing changes:', error);
    }
  };

  const validateConfiguration = async () => {
    // Create a temporary configuration for validation
    const tempConfig: PriorityConfigurationType = {
      id: 0,
      businessVerticalId: selectedBusinessVertical,
      priorityName: 'Test Configuration',
      minScore: 0.0,
      maxScore: 1.0,
      colorCode: '#000000',
      iconClass: '',
      description: 'Test configuration for validation',
      isActive: true,
      escalationRules: '{}',
      timeDecayConfiguration: JSON.stringify(timeDecayConfig),
      businessValueWeights: JSON.stringify(businessValueConfig),
      capacityFactors: JSON.stringify(capacityConfig),
      autoAdjustmentRules: '{}',
      slaHours: 72,
      escalationThresholdHours: 48,
      notificationSettings: '{}',
      createdDate: new Date().toISOString(),
      modifiedDate: new Date().toISOString(),
      createdBy: 'System',
      modifiedBy: 'System'
    };

    try {
      const response = await apiService.post('/api/priorityconfiguration/validate', tempConfig);
      setValidationResult(response.data);
    } catch (error) {
      console.error('Error validating configuration:', error);
    }
  };

  const updateAlgorithmConfig = (field: keyof PriorityAlgorithmConfig, value: any) => {
    setAlgorithmConfig(prev => ({ ...prev, [field]: value }));
    setHasUnsavedChanges(true);
  };

  const updateTimeDecayConfig = (field: keyof TimeDecayConfig, value: any) => {
    setTimeDecayConfig(prev => ({ ...prev, [field]: value }));
    setHasUnsavedChanges(true);
  };

  const updateBusinessValueConfig = (field: keyof BusinessValueWeightConfig, value: any) => {
    setBusinessValueConfig(prev => ({ ...prev, [field]: value }));
    setHasUnsavedChanges(true);
  };

  const updateCapacityConfig = (field: keyof CapacityAdjustmentConfig, value: any) => {
    setCapacityConfig(prev => ({ ...prev, [field]: value }));
    setHasUnsavedChanges(true);
  };

  const tabs = [
    { id: 0, name: 'Algorithm Configuration', icon: '⚙️' },
    { id: 1, name: 'Time Decay Settings', icon: '⏰' },
    { id: 2, name: 'Business Value Weights', icon: '💼' },
    { id: 3, name: 'Capacity Adjustment', icon: '📊' },
    { id: 4, name: 'Preview & Analytics', icon: '📈' }
  ];

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <h1 className="text-2xl font-bold text-gray-900">Priority Configuration</h1>
            
            {/* Business Vertical Selector */}
            <select
              value={selectedBusinessVertical}
              onChange={(e) => setSelectedBusinessVertical(parseInt(e.target.value))}
              className="rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            >
              <option value={0}>Select Business Vertical</option>
              {businessVerticals.map(vertical => (
                <option key={vertical.id} value={vertical.id}>
                  {vertical.name}
                </option>
              ))}
            </select>
          </div>

          <div className="flex items-center space-x-2">
            <button
              onClick={validateConfiguration}
              disabled={selectedBusinessVertical === 0}
              className="px-3 py-2 bg-yellow-600 text-white rounded-md hover:bg-yellow-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Validate
            </button>
            
            <button
              onClick={previewChanges}
              disabled={selectedBusinessVertical === 0 || !hasUnsavedChanges}
              className="px-3 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Preview Changes
            </button>
            
            <button
              onClick={saveConfiguration}
              disabled={selectedBusinessVertical === 0 || !hasUnsavedChanges || saving}
              className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {saving ? 'Saving...' : 'Save Configuration'}
            </button>
          </div>
        </div>

        {/* Tabs */}
        <div className="mt-4">
          <nav className="flex space-x-8" aria-label="Tabs">
            {tabs.map(tab => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`py-2 px-1 border-b-2 font-medium text-sm flex items-center space-x-2 ${
                  activeTab === tab.id
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <span>{tab.icon}</span>
                <span>{tab.name}</span>
              </button>
            ))}
          </nav>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto p-6">
        {selectedBusinessVertical === 0 ? (
          <div className="text-center py-12">
            <div className="text-gray-400 text-6xl mb-4">⚙️</div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">Select Business Vertical</h3>
            <p className="text-gray-500">Choose a business vertical to configure priority settings.</p>
          </div>
        ) : (
          <>
            {/* Algorithm Configuration Tab */}
            {activeTab === 0 && (
              <AlgorithmConfigurationPanel
                config={algorithmConfig}
                onUpdate={updateAlgorithmConfig}
                validationResult={validationResult}
              />
            )}

            {/* Time Decay Settings Tab */}
            {activeTab === 1 && (
              <TimeDecayConfigurationPanel
                config={timeDecayConfig}
                onUpdate={updateTimeDecayConfig}
                businessVerticalId={selectedBusinessVertical}
              />
            )}

            {/* Business Value Weights Tab */}
            {activeTab === 2 && (
              <BusinessValueConfigurationPanel
                config={businessValueConfig}
                onUpdate={updateBusinessValueConfig}
              />
            )}

            {/* Capacity Adjustment Tab */}
            {activeTab === 3 && (
              <CapacityAdjustmentPanel
                config={capacityConfig}
                onUpdate={updateCapacityConfig}
              />
            )}

            {/* Preview & Analytics Tab */}
            {activeTab === 4 && (
              <PreviewAnalyticsPanel
                previewResult={previewResult}
                trendAnalysis={trendAnalysis}
                effectivenessMetrics={effectivenessMetrics}
                recommendations={recommendations}
                onPreview={previewChanges}
              />
            )}
          </>
        )}
      </div>

      {/* Unsaved Changes Warning */}
      {hasUnsavedChanges && (
        <div className="bg-yellow-50 border-t border-yellow-200 px-6 py-3">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <span className="text-yellow-400">⚠️</span>
            </div>
            <div className="ml-3">
              <p className="text-sm text-yellow-700">
                You have unsaved changes. Don't forget to save your configuration.
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

// Algorithm Configuration Panel Component
const AlgorithmConfigurationPanel: React.FC<{
  config: PriorityAlgorithmConfig;
  onUpdate: (field: keyof PriorityAlgorithmConfig, value: any) => void;
  validationResult: PriorityConfigValidationResult | null;
}> = ({ config, onUpdate, validationResult }) => {
  return (
    <div className="space-y-6">
      <div className="bg-white shadow rounded-lg p-6">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Priority Algorithm Settings</h3>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700">Algorithm Type</label>
            <select
              value={config.algorithmType}
              onChange={(e) => onUpdate('algorithmType', e.target.value as 'Enhanced' | 'Simple' | 'Custom')}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            >
              <option value="Enhanced">Enhanced (Recommended)</option>
              <option value="Simple">Simple</option>
              <option value="Custom">Custom</option>
            </select>
            <p className="mt-1 text-sm text-gray-500">
              Enhanced algorithm considers all factors with optimal weighting
            </p>
          </div>

          <div className="flex items-center">
            <input
              type="checkbox"
              id="algorithmActive"
              checked={config.isActive}
              onChange={(e) => onUpdate('isActive', e.target.checked)}
              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <label htmlFor="algorithmActive" className="ml-2 block text-sm text-gray-900">
              Algorithm Active
            </label>
          </div>
        </div>

        <div className="mt-6">
          <h4 className="text-md font-medium text-gray-900 mb-4">Weight Configuration</h4>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Base Weight</label>
              <input
                type="number"
                step="0.1"
                min="0"
                max="5"
                value={config.baseWeight}
                onChange={(e) => onUpdate('baseWeight', parseFloat(e.target.value) || 0)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Time Decay Weight</label>
              <input
                type="number"
                step="0.1"
                min="0"
                max="5"
                value={config.timeDecayWeight}
                onChange={(e) => onUpdate('timeDecayWeight', parseFloat(e.target.value) || 0)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Business Value Weight</label>
              <input
                type="number"
                step="0.1"
                min="0"
                max="5"
                value={config.businessValueWeight}
                onChange={(e) => onUpdate('businessValueWeight', parseFloat(e.target.value) || 0)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Capacity Adjustment Weight</label>
              <input
                type="number"
                step="0.1"
                min="0"
                max="5"
                value={config.capacityAdjustmentWeight}
                onChange={(e) => onUpdate('capacityAdjustmentWeight', parseFloat(e.target.value) || 0)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>
          </div>
        </div>

        {/* Validation Results */}
        {validationResult && (
          <div className="mt-6">
            <div className={`p-4 rounded-md ${
              validationResult.isValid ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'
            }`}>
              <div className="flex">
                <div className="flex-shrink-0">
                  {validationResult.isValid ? (
                    <span className="text-green-500">✓</span>
                  ) : (
                    <span className="text-red-500">✗</span>
                  )}
                </div>
                <div className="ml-3">
                  <h3 className={`text-sm font-medium ${
                    validationResult.isValid ? 'text-green-800' : 'text-red-800'
                  }`}>
                    {validationResult.isValid ? 'Configuration is valid' : 'Configuration has errors'}
                  </h3>
                  {validationResult.errors.length > 0 && (
                    <ul className="mt-2 text-sm text-red-700">
                      {validationResult.errors.map((error, index) => (
                        <li key={index}>• {error}</li>
                      ))}
                    </ul>
                  )}
                  {validationResult.warnings.length > 0 && (
                    <ul className="mt-2 text-sm text-yellow-700">
                      {validationResult.warnings.map((warning, index) => (
                        <li key={index}>⚠️ {warning}</li>
                      ))}
                    </ul>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

// Time Decay Configuration Panel Component
const TimeDecayConfigurationPanel: React.FC<{
  config: TimeDecayConfig;
  onUpdate: (field: keyof TimeDecayConfig, value: any) => void;
  businessVerticalId: number;
}> = ({ config, onUpdate, businessVerticalId }) => {
  const [testDate, setTestDate] = useState(new Date().toISOString().split('T')[0]);
  const [testResult, setTestResult] = useState<any>(null);

  const testTimeDecay = async () => {
    try {
      const response = await apiService.post(`/api/priorityconfiguration/time-decay/${businessVerticalId}/calculate`, new Date(testDate));
      setTestResult(response.data);
    } catch (error) {
      console.error('Error testing time decay:', error);
    }
  };

  return (
    <div className="space-y-6">
      <div className="bg-white shadow rounded-lg p-6">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Time Decay Configuration</h3>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="flex items-center">
            <input
              type="checkbox"
              id="timeDecayEnabled"
              checked={config.isEnabled}
              onChange={(e) => onUpdate('isEnabled', e.target.checked)}
              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <label htmlFor="timeDecayEnabled" className="ml-2 block text-sm text-gray-900">
              Enable Time Decay
            </label>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Decay Function</label>
            <select
              value={config.decayFunction}
              onChange={(e) => onUpdate('decayFunction', e.target.value as 'Linear' | 'Logarithmic' | 'Exponential')}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            >
              <option value="Linear">Linear</option>
              <option value="Logarithmic">Logarithmic (Recommended)</option>
              <option value="Exponential">Exponential</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Max Multiplier</label>
            <input
              type="number"
              step="0.1"
              min="1"
              max="10"
              value={config.maxMultiplier}
              onChange={(e) => onUpdate('maxMultiplier', parseFloat(e.target.value) || 1)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">Maximum priority multiplier (e.g., 2.0 = 200%)</p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Decay Rate</label>
            <input
              type="number"
              step="0.001"
              min="0.001"
              max="1"
              value={config.decayRate}
              onChange={(e) => onUpdate('decayRate', parseFloat(e.target.value) || 0.01)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">Daily decay rate (0.01 = 1% per day)</p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Start Delay (Days)</label>
            <input
              type="number"
              min="0"
              max="365"
              value={config.startDelayDays}
              onChange={(e) => onUpdate('startDelayDays', parseInt(e.target.value) || 0)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">Days before decay starts</p>
          </div>
        </div>

        {/* Time Decay Test */}
        <div className="mt-6 p-4 bg-gray-50 rounded-lg">
          <h4 className="text-md font-medium text-gray-900 mb-3">Test Time Decay Calculation</h4>
          <div className="flex items-end space-x-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Created Date</label>
              <input
                type="date"
                value={testDate}
                onChange={(e) => setTestDate(e.target.value)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>
            <button
              onClick={testTimeDecay}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
            >
              Calculate
            </button>
          </div>
          
          {testResult && (
            <div className="mt-4 p-3 bg-white rounded border">
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                <div>
                  <span className="font-medium">Days Old:</span>
                  <div>{testResult.daysOld} days</div>
                </div>
                <div>
                  <span className="font-medium">Decay Factor:</span>
                  <div className="text-blue-600 font-bold">{testResult.timeDecayFactor.toFixed(3)}x</div>
                </div>
                <div>
                  <span className="font-medium">Priority Boost:</span>
                  <div className="text-green-600">+{((testResult.timeDecayFactor - 1) * 100).toFixed(1)}%</div>
                </div>
                <div>
                  <span className="font-medium">Function:</span>
                  <div>{testResult.configuration.decayFunction}</div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

// Business Value Configuration Panel Component
const BusinessValueConfigurationPanel: React.FC<{
  config: BusinessValueWeightConfig;
  onUpdate: (field: keyof BusinessValueWeightConfig, value: any) => void;
}> = ({ config, onUpdate }) => {
  return (
    <div className="space-y-6">
      <div className="bg-white shadow rounded-lg p-6">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Business Value Weight Configuration</h3>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700">Base Multiplier</label>
            <input
              type="number"
              step="0.1"
              min="0.1"
              max="5"
              value={config.baseMultiplier}
              onChange={(e) => onUpdate('baseMultiplier', parseFloat(e.target.value) || 1)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">Base multiplier for all business value calculations</p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Strategic Alignment Multiplier</label>
            <input
              type="number"
              step="0.1"
              min="1"
              max="3"
              value={config.strategicAlignmentMultiplier}
              onChange={(e) => onUpdate('strategicAlignmentMultiplier', parseFloat(e.target.value) || 1.2)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">Multiplier for strategically aligned work requests</p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">ROI Threshold</label>
            <input
              type="number"
              step="0.01"
              min="0"
              max="1"
              value={config.roiThreshold}
              onChange={(e) => onUpdate('roiThreshold', parseFloat(e.target.value) || 0.15)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">ROI threshold for bonus multiplier (0.15 = 15%)</p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">ROI Bonus Multiplier</label>
            <input
              type="number"
              step="0.1"
              min="1"
              max="3"
              value={config.roiBonusMultiplier}
              onChange={(e) => onUpdate('roiBonusMultiplier', parseFloat(e.target.value) || 1.5)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">Bonus multiplier for high-ROI work requests</p>
          </div>
        </div>

        {/* Category Weights Section */}
        <div className="mt-6">
          <h4 className="text-md font-medium text-gray-900 mb-3">Category-Specific Weights</h4>
          <div className="bg-gray-50 p-4 rounded-lg">
            <p className="text-sm text-gray-600 mb-2">Configure different weights for work request categories:</p>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Work Request</label>
                <input
                  type="number"
                  step="0.1"
                  min="0.1"
                  max="3"
                  value={config.categoryWeights['WorkRequest'] || 1.0}
                  onChange={(e) => onUpdate('categoryWeights', { 
                    ...config.categoryWeights, 
                    'WorkRequest': parseFloat(e.target.value) || 1.0 
                  })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Project</label>
                <input
                  type="number"
                  step="0.1"
                  min="0.1"
                  max="3"
                  value={config.categoryWeights['Project'] || 1.2}
                  onChange={(e) => onUpdate('categoryWeights', { 
                    ...config.categoryWeights, 
                    'Project': parseFloat(e.target.value) || 1.2 
                  })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Break/Fix</label>
                <input
                  type="number"
                  step="0.1"
                  min="0.1"
                  max="3"
                  value={config.categoryWeights['BreakFix'] || 0.8}
                  onChange={(e) => onUpdate('categoryWeights', { 
                    ...config.categoryWeights, 
                    'BreakFix': parseFloat(e.target.value) || 0.8 
                  })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// Capacity Adjustment Panel Component
const CapacityAdjustmentPanel: React.FC<{
  config: CapacityAdjustmentConfig;
  onUpdate: (field: keyof CapacityAdjustmentConfig, value: any) => void;
}> = ({ config, onUpdate }) => {
  return (
    <div className="space-y-6">
      <div className="bg-white shadow rounded-lg p-6">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Capacity Adjustment Configuration</h3>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="flex items-center">
            <input
              type="checkbox"
              id="capacityEnabled"
              checked={config.isEnabled}
              onChange={(e) => onUpdate('isEnabled', e.target.checked)}
              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <label htmlFor="capacityEnabled" className="ml-2 block text-sm text-gray-900">
              Enable Capacity Adjustment
            </label>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Adjustment Curve</label>
            <select
              value={config.adjustmentCurve}
              onChange={(e) => onUpdate('adjustmentCurve', e.target.value as 'Linear' | 'Sigmoid' | 'Step')}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            >
              <option value="Linear">Linear</option>
              <option value="Sigmoid">Sigmoid (S-Curve)</option>
              <option value="Step">Step Function</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Max Adjustment Factor</label>
            <input
              type="number"
              step="0.1"
              min="1"
              max="3"
              value={config.maxAdjustmentFactor}
              onChange={(e) => onUpdate('maxAdjustmentFactor', parseFloat(e.target.value) || 1.5)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">Maximum priority boost for low utilization</p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Min Adjustment Factor</label>
            <input
              type="number"
              step="0.1"
              min="0.1"
              max="1"
              value={config.minAdjustmentFactor}
              onChange={(e) => onUpdate('minAdjustmentFactor', parseFloat(e.target.value) || 0.5)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">Minimum priority penalty for high utilization</p>
          </div>

          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700">Optimal Utilization (%)</label>
            <input
              type="number"
              min="10"
              max="100"
              value={config.optimalUtilizationPercentage}
              onChange={(e) => onUpdate('optimalUtilizationPercentage', parseFloat(e.target.value) || 80)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-sm text-gray-500">Target utilization percentage (80% recommended)</p>
          </div>
        </div>

        {/* Capacity Visualization */}
        <div className="mt-6">
          <h4 className="text-md font-medium text-gray-900 mb-3">Adjustment Curve Visualization</h4>
          <div className="bg-gray-50 p-4 rounded-lg">
            <div className="grid grid-cols-5 gap-2 text-center text-sm">
              <div className="font-medium">Utilization</div>
              <div className="font-medium">Adjustment</div>
              <div className="font-medium">Priority Impact</div>
              <div className="font-medium">Status</div>
              <div className="font-medium">Recommendation</div>
              
              <div>20%</div>
              <div className="text-green-600 font-bold">{config.maxAdjustmentFactor}x</div>
              <div className="text-green-600">+{((config.maxAdjustmentFactor - 1) * 100).toFixed(0)}%</div>
              <div className="text-green-600">Under-utilized</div>
              <div>Higher priority</div>
              
              <div>50%</div>
              <div className="text-blue-600 font-bold">1.2x</div>
              <div className="text-blue-600">+20%</div>
              <div className="text-blue-600">Good capacity</div>
              <div>Slight boost</div>
              
              <div>{config.optimalUtilizationPercentage}%</div>
              <div className="font-bold">1.0x</div>
              <div>No change</div>
              <div>Optimal</div>
              <div>Baseline priority</div>
              
              <div>90%</div>
              <div className="text-orange-600 font-bold">0.8x</div>
              <div className="text-orange-600">-20%</div>
              <div className="text-orange-600">Near capacity</div>
              <div>Lower priority</div>
              
              <div>100%</div>
              <div className="text-red-600 font-bold">{config.minAdjustmentFactor}x</div>
              <div className="text-red-600">-{((1 - config.minAdjustmentFactor) * 100).toFixed(0)}%</div>
              <div className="text-red-600">Over-utilized</div>
              <div>Much lower priority</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// Preview & Analytics Panel Component
const PreviewAnalyticsPanel: React.FC<{
  previewResult: PriorityPreviewResult | null;
  trendAnalysis: PriorityTrendAnalysis | null;
  effectivenessMetrics: PriorityEffectivenessMetric[];
  recommendations: PriorityRecommendation | null;
  onPreview: () => void;
}> = ({ previewResult, trendAnalysis, effectivenessMetrics, recommendations, onPreview }) => {
  return (
    <div className="space-y-6">
      {/* Preview Results */}
      <div className="bg-white shadow rounded-lg p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-medium text-gray-900">Configuration Preview</h3>
          <button
            onClick={onPreview}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Generate Preview
          </button>
        </div>
        
        {previewResult ? (
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="text-center p-4 bg-blue-50 rounded-lg">
                <div className="text-2xl font-bold text-blue-600">{previewResult.totalWorkRequests}</div>
                <div className="text-sm text-blue-800">Total Work Requests</div>
              </div>
              <div className="text-center p-4 bg-green-50 rounded-lg">
                <div className="text-2xl font-bold text-green-600">{previewResult.affectedWorkRequests}</div>
                <div className="text-sm text-green-800">Affected by Changes</div>
              </div>
              <div className="text-center p-4 bg-yellow-50 rounded-lg">
                <div className="text-2xl font-bold text-yellow-600">
                  {previewResult.totalWorkRequests > 0 
                    ? ((previewResult.affectedWorkRequests / previewResult.totalWorkRequests) * 100).toFixed(1)
                    : 0}%
                </div>
                <div className="text-sm text-yellow-800">Impact Percentage</div>
              </div>
            </div>

            {previewResult.changes.length > 0 && (
              <div>
                <h4 className="text-md font-medium text-gray-900 mb-2">Significant Changes</h4>
                <div className="max-h-48 overflow-y-auto">
                  <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase">Work Request</th>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase">Current</th>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase">New</th>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase">Change</th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {previewResult.changes.slice(0, 10).map(change => (
                        <tr key={change.workRequestId}>
                          <td className="px-3 py-2 text-sm">
                            <div className="truncate max-w-48" title={change.title}>
                              {change.title}
                            </div>
                          </td>
                          <td className="px-3 py-2 text-sm">{change.currentPriority.toFixed(3)}</td>
                          <td className="px-3 py-2 text-sm">{change.newPriority.toFixed(3)}</td>
                          <td className="px-3 py-2 text-sm">
                            <span className={`font-medium ${
                              change.change > 0 ? 'text-green-600' : 'text-red-600'
                            }`}>
                              {change.change > 0 ? '+' : ''}{change.change.toFixed(3)}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            <div className="text-4xl mb-2">📊</div>
            <p>Click "Generate Preview" to see how your changes will affect work request priorities</p>
          </div>
        )}
      </div>

      {/* Effectiveness Metrics */}
      {effectivenessMetrics.length > 0 && (
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Effectiveness Metrics</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {effectivenessMetrics.map((metric, index) => (
              <div key={index} className="text-center p-4 bg-gray-50 rounded-lg">
                <div className="text-xl font-bold text-gray-900">{metric.value.toFixed(2)} {metric.unit}</div>
                <div className="text-sm font-medium text-gray-700">{metric.metricName}</div>
                <div className="text-xs text-gray-500 mt-1">{metric.description}</div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Recommendations */}
      {recommendations && (
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Configuration Recommendations</h3>
          <div className="space-y-4">
            <div className="flex items-start">
              <div className="flex-shrink-0">
                <div className="text-2xl">💡</div>
              </div>
              <div className="ml-3">
                <h4 className="text-md font-medium text-gray-900">{recommendations.title}</h4>
                <p className="text-sm text-gray-600 mt-1">{recommendations.description}</p>
                <p className="text-xs text-gray-500 mt-2">{recommendations.rationale}</p>
                <div className="mt-2">
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                    Confidence: {(recommendations.confidenceScore * 100).toFixed(0)}%
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Trend Analysis */}
      {trendAnalysis && (
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Priority Trends</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="text-center p-4 bg-blue-50 rounded-lg">
              <div className="text-xl font-bold text-blue-600">{trendAnalysis.averagePriorityScore.toFixed(3)}</div>
              <div className="text-sm text-blue-800">Average Priority Score</div>
            </div>
            <div className="text-center p-4 bg-purple-50 rounded-lg">
              <div className="text-xl font-bold text-purple-600">{trendAnalysis.priorityVolatility.toFixed(3)}</div>
              <div className="text-sm text-purple-800">Priority Volatility</div>
            </div>
          </div>
          
          {trendAnalysis.insights.length > 0 && (
            <div className="mt-4">
              <h4 className="text-md font-medium text-gray-900 mb-2">Insights</h4>
              <ul className="space-y-1">
                {trendAnalysis.insights.map((insight, index) => (
                  <li key={index} className="text-sm text-gray-600 flex items-start">
                    <span className="text-blue-500 mr-2">•</span>
                    {insight}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default PriorityConfiguration; 