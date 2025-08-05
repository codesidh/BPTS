import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  WorkflowStageConfiguration,
  WorkflowTransition,
  WorkflowDesignerState,
  WorkflowValidationResult,
  UserRole,
  BusinessVertical
} from '../types';
import { apiService } from '../services/api';

interface WorkflowDesignerProps {
  businessVerticalId?: number;
}

const WorkflowDesigner: React.FC<WorkflowDesignerProps> = ({ businessVerticalId }) => {
  // Core state
  const [designerState, setDesignerState] = useState<WorkflowDesignerState>({
    stages: [],
    transitions: [],
    selectedStage: undefined,
    selectedTransition: undefined,
    isDragging: false,
    isConnecting: false,
    businessVerticalId,
    validationResults: {
      isValid: true,
      validationErrors: [],
      warnings: [],
      validationDate: new Date().toISOString()
    },
    hasUnsavedChanges: false,
    previewMode: false,
    gridSnap: true,
    zoomLevel: 1.0
  });

  // UI state
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [businessVerticals, setBusinessVerticals] = useState<BusinessVertical[]>([]);
  const [showStageProperties, setShowStageProperties] = useState(false);
  const [showTransitionProperties, setShowTransitionProperties] = useState(false);
  const [showValidationPanel, setShowValidationPanel] = useState(false);
  const [dragOffset, setDragOffset] = useState({ x: 0, y: 0 });
  const [connectionStart, setConnectionStart] = useState<{ stageId: number; x: number; y: number } | null>(null);

  // Refs
  const canvasRef = useRef<HTMLDivElement>(null);
  const stageRefs = useRef<Map<number, HTMLDivElement>>(new Map());

  // Load initial data
  useEffect(() => {
    loadWorkflowData();
    loadBusinessVerticals();
  }, [businessVerticalId]);

  const loadWorkflowData = async () => {
    try {
      setLoading(true);
      const [stagesResponse, transitionsResponse] = await Promise.all([
        apiService.get(`/api/workflow/stages${businessVerticalId ? `?businessVerticalId=${businessVerticalId}` : ''}`),
        apiService.get(`/api/workflow/transitions${businessVerticalId ? `?businessVerticalId=${businessVerticalId}` : ''}`)
      ]);

      const stages = stagesResponse.data.map((stage: any, index: number) => ({
        ...stage,
        x: stage.x || (index * 200 + 100),
        y: stage.y || (Math.floor(index / 4) * 150 + 100),
        width: 180,
        height: 100
      }));

      setDesignerState(prev => ({
        ...prev,
        stages,
        transitions: transitionsResponse.data
      }));

      // Validate workflow on load
      await validateWorkflow();
    } catch (error) {
      console.error('Error loading workflow data:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadBusinessVerticals = async () => {
    try {
      const response = await apiService.get('/api/business-verticals');
      setBusinessVerticals(response.data);
    } catch (error) {
      console.error('Error loading business verticals:', error);
    }
  };

  const validateWorkflow = async () => {
    try {
      const response = await apiService.get(`/api/workflow/validate${businessVerticalId ? `?businessVerticalId=${businessVerticalId}` : ''}`);
      setDesignerState(prev => ({
        ...prev,
        validationResults: response.data
      }));
    } catch (error) {
      console.error('Error validating workflow:', error);
    }
  };

  // Stage management
  const createStage = () => {
    const newStage: WorkflowStageConfiguration = {
      id: Date.now(), // Temporary ID
      name: `New Stage ${designerState.stages.length + 1}`,
      order: designerState.stages.length + 1,
      description: '',
      businessVerticalId,
      requiredRoles: [],
      approvalRequired: false,
      isActive: true,
      stageType: 'Standard',
      notificationTemplate: {
        subject: '',
        body: '',
        recipients: [],
        templateVariables: {},
        isActive: false
      },
      autoTransition: false,
      allowedTransitions: [],
      validationRules: {
        rules: [],
        isStrict: false,
        errorMessages: {}
      },
      version: 1,
      changeHistory: [],
      x: 100 + (designerState.stages.length * 220),
      y: 100,
      width: 180,
      height: 100
    };

    setDesignerState(prev => ({
      ...prev,
      stages: [...prev.stages, newStage],
      selectedStage: newStage,
      hasUnsavedChanges: true
    }));
    setShowStageProperties(true);
  };

  const updateStage = (updatedStage: WorkflowStageConfiguration) => {
    setDesignerState(prev => ({
      ...prev,
      stages: prev.stages.map(stage =>
        stage.id === updatedStage.id ? updatedStage : stage
      ),
      selectedStage: updatedStage,
      hasUnsavedChanges: true
    }));
  };

  const deleteStage = (stageId: number) => {
    if (window.confirm('Are you sure you want to delete this stage? This will also delete all associated transitions.')) {
      setDesignerState(prev => ({
        ...prev,
        stages: prev.stages.filter(stage => stage.id !== stageId),
        transitions: prev.transitions.filter(
          transition => transition.fromStageId !== stageId && transition.toStageId !== stageId
        ),
        selectedStage: undefined,
        hasUnsavedChanges: true
      }));
      setShowStageProperties(false);
    }
  };

  // Transition management
  const createTransition = (fromStageId: number, toStageId: number) => {
    const fromStage = designerState.stages.find(s => s.id === fromStageId);
    const toStage = designerState.stages.find(s => s.id === toStageId);
    
    if (!fromStage || !toStage) return;

    const newTransition: WorkflowTransition = {
      id: Date.now(), // Temporary ID
      fromStageId,
      toStageId,
      businessVerticalId,
      transitionName: `${fromStage.name} → ${toStage.name}`,
      isActive: true,
      conditionScript: {
        type: 'custom',
        conditions: [],
        operator: 'and',
        isActive: false
      },
      notificationRequired: false,
      notificationTemplate: {
        subject: '',
        body: '',
        recipients: [],
        templateVariables: {},
        isActive: false
      },
      validationRules: {
        rules: [],
        isStrict: false,
        errorMessages: {},
        preConditions: [],
        postConditions: []
      }
    };

    setDesignerState(prev => ({
      ...prev,
      transitions: [...prev.transitions, newTransition],
      selectedTransition: newTransition,
      hasUnsavedChanges: true
    }));
    setShowTransitionProperties(true);
  };

  const updateTransition = (updatedTransition: WorkflowTransition) => {
    setDesignerState(prev => ({
      ...prev,
      transitions: prev.transitions.map(transition =>
        transition.id === updatedTransition.id ? updatedTransition : transition
      ),
      selectedTransition: updatedTransition,
      hasUnsavedChanges: true
    }));
  };

  // const deleteTransition = (transitionId: number) => {
  //   if (window.confirm('Are you sure you want to delete this transition?')) {
  //     setDesignerState(prev => ({
  //       ...prev,
  //       transitions: prev.transitions.filter(transition => transition.id !== transitionId),
  //       selectedTransition: undefined,
  //       hasUnsavedChanges: true
  //     }));
  //     setShowTransitionProperties(false);
  //   }
  // };

  // Drag and drop functionality
  const handleStageMouseDown = (e: React.MouseEvent, stage: WorkflowStageConfiguration) => {
    if (designerState.previewMode) return;

    const rect = e.currentTarget.getBoundingClientRect();
    setDragOffset({
      x: e.clientX - rect.left,
      y: e.clientY - rect.top
    });

    setDesignerState(prev => ({
      ...prev,
      selectedStage: stage,
      isDragging: true
    }));
    setShowStageProperties(true);
  };

  const handleMouseMove = useCallback((e: MouseEvent) => {
    if (!designerState.isDragging || !designerState.selectedStage || !canvasRef.current) return;

    const canvasRect = canvasRef.current.getBoundingClientRect();
    let newX = (e.clientX - canvasRect.left - dragOffset.x) / designerState.zoomLevel;
    let newY = (e.clientY - canvasRect.top - dragOffset.y) / designerState.zoomLevel;

    // Grid snapping
    if (designerState.gridSnap) {
      newX = Math.round(newX / 20) * 20;
      newY = Math.round(newY / 20) * 20;
    }

    const updatedStage = {
      ...designerState.selectedStage,
      x: Math.max(0, newX),
      y: Math.max(0, newY)
    };

    updateStage(updatedStage);
  }, [designerState.isDragging, designerState.selectedStage, designerState.gridSnap, designerState.zoomLevel, dragOffset]);

  const handleMouseUp = useCallback(() => {
    setDesignerState(prev => ({
      ...prev,
      isDragging: false,
      isConnecting: false
    }));
    setConnectionStart(null);
  }, []);

  useEffect(() => {
    if (designerState.isDragging) {
      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', handleMouseUp);
      return () => {
        document.removeEventListener('mousemove', handleMouseMove);
        document.removeEventListener('mouseup', handleMouseUp);
      };
    }
  }, [designerState.isDragging, handleMouseMove, handleMouseUp]);

  // Connection functionality
  const handleConnectorMouseDown = (e: React.MouseEvent, stage: WorkflowStageConfiguration) => {
    e.stopPropagation();
    if (designerState.previewMode) return;

    const rect = e.currentTarget.getBoundingClientRect();
    setConnectionStart({
      stageId: stage.id,
      x: rect.left + rect.width / 2,
      y: rect.top + rect.height / 2
    });

    setDesignerState(prev => ({
      ...prev,
      isConnecting: true
    }));
  };

  const handleStageMouseUp = (_e: React.MouseEvent, stage: WorkflowStageConfiguration) => {
    if (designerState.isConnecting && connectionStart && connectionStart.stageId !== stage.id) {
      createTransition(connectionStart.stageId, stage.id);
    }
    setConnectionStart(null);
    setDesignerState(prev => ({
      ...prev,
      isConnecting: false
    }));
  };

  // Save workflow
  const saveWorkflow = async () => {
    if (!designerState.hasUnsavedChanges) return;

    try {
      setSaving(true);
      
      // Save stages
      const stagePromises = designerState.stages.map(stage => {
        if (stage.id < 1000000) { // Temporary ID, this is a new stage
          return apiService.post('/api/workflow/stages', stage);
        } else {
          return apiService.put(`/api/workflow/stages/${stage.id}`, stage);
        }
      });

      // Save transitions
      const transitionPromises = designerState.transitions.map(transition => {
        if (transition.id < 1000000) { // Temporary ID, this is a new transition
          return apiService.post('/api/workflow/transitions', transition);
        } else {
          return apiService.put(`/api/workflow/transitions/${transition.id}`, transition);
        }
      });

      await Promise.all([...stagePromises, ...transitionPromises]);

      setDesignerState(prev => ({
        ...prev,
        hasUnsavedChanges: false
      }));

      // Refresh data and validate
      await loadWorkflowData();
      alert('Workflow saved successfully!');
    } catch (error) {
      console.error('Error saving workflow:', error);
      alert('Error saving workflow. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  // Render SVG arrows for transitions
  const renderTransitions = () => {
    return designerState.transitions.map(transition => {
      const fromStage = designerState.stages.find(s => s.id === transition.fromStageId);
      const toStage = designerState.stages.find(s => s.id === transition.toStageId);
      
      if (!fromStage || !toStage) return null;

      const fromX = (fromStage.x || 0) + (fromStage.width || 180) / 2;
      const fromY = (fromStage.y || 0) + (fromStage.height || 100);
      const toX = (toStage.x || 0) + (toStage.width || 180) / 2;
      const toY = (toStage.y || 0);

      const isSelected = designerState.selectedTransition?.id === transition.id;

      return (
        <g key={transition.id}>
          <line
            x1={fromX}
            y1={fromY}
            x2={toX}
            y2={toY}
            stroke={isSelected ? '#3b82f6' : '#6b7280'}
            strokeWidth={isSelected ? 3 : 2}
            markerEnd="url(#arrowhead)"
            className="cursor-pointer hover:stroke-blue-500"
            onClick={() => {
              setDesignerState(prev => ({ ...prev, selectedTransition: transition }));
              setShowTransitionProperties(true);
            }}
          />
          {/* Transition label */}
          <text
            x={(fromX + toX) / 2}
            y={(fromY + toY) / 2}
            textAnchor="middle"
            className="fill-gray-600 text-xs font-medium pointer-events-none"
            dy="-5"
          >
            {transition.transitionName}
          </text>
        </g>
      );
    });
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Toolbar */}
      <div className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <h1 className="text-2xl font-bold text-gray-900">Workflow Designer</h1>
          
          {/* Business Vertical Selector */}
          <select
            value={designerState.businessVerticalId || ''}
            onChange={(e) => {
              const newVerticalId = e.target.value ? parseInt(e.target.value) : undefined;
              setDesignerState(prev => ({ ...prev, businessVerticalId: newVerticalId }));
              // TODO: Reload workflow data for new vertical
            }}
            className="rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
          >
            <option value="">Global Workflow</option>
            {businessVerticals.map(vertical => (
              <option key={vertical.id} value={vertical.id}>
                {vertical.name}
              </option>
            ))}
          </select>
        </div>

        <div className="flex items-center space-x-2">
          {/* Toolbar buttons */}
          <button
            onClick={createStage}
            disabled={designerState.previewMode}
            className="px-3 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Add Stage
          </button>
          
          <button
            onClick={() => setShowValidationPanel(!showValidationPanel)}
            className={`px-3 py-2 rounded-md ${
              designerState.validationResults.isValid 
                ? 'bg-green-100 text-green-800 hover:bg-green-200' 
                : 'bg-red-100 text-red-800 hover:bg-red-200'
            }`}
          >
            Validate ({designerState.validationResults.validationErrors.length} errors)
          </button>
          
          <button
            onClick={() => setDesignerState(prev => ({ ...prev, previewMode: !prev.previewMode }))}
            className={`px-3 py-2 rounded-md ${
              designerState.previewMode 
                ? 'bg-orange-100 text-orange-800 hover:bg-orange-200' 
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            {designerState.previewMode ? 'Exit Preview' : 'Preview'}
          </button>
          
          <button
            onClick={saveWorkflow}
            disabled={!designerState.hasUnsavedChanges || saving}
            className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {saving ? 'Saving...' : 'Save Workflow'}
          </button>
          
          {/* Zoom controls */}
          <div className="flex items-center space-x-1 border-l pl-2">
            <button
              onClick={() => setDesignerState(prev => ({ ...prev, zoomLevel: Math.max(0.1, prev.zoomLevel - 0.1) }))}
              className="p-1 text-gray-600 hover:text-gray-900"
            >
              🔍-
            </button>
            <span className="text-sm text-gray-600 min-w-12 text-center">
              {Math.round(designerState.zoomLevel * 100)}%
            </span>
            <button
              onClick={() => setDesignerState(prev => ({ ...prev, zoomLevel: Math.min(2.0, prev.zoomLevel + 0.1) }))}
              className="p-1 text-gray-600 hover:text-gray-900"
            >
              🔍+
            </button>
          </div>
          
          <label className="flex items-center space-x-2">
            <input
              type="checkbox"
              checked={designerState.gridSnap}
              onChange={(e) => setDesignerState(prev => ({ ...prev, gridSnap: e.target.checked }))}
              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <span className="text-sm text-gray-600">Grid Snap</span>
          </label>
        </div>
      </div>

      {/* Main content */}
      <div className="flex-1 flex">
        {/* Canvas */}
        <div className="flex-1 relative overflow-auto bg-gray-100">
          <div
            ref={canvasRef}
            className="relative"
            style={{
              transform: `scale(${designerState.zoomLevel})`,
              transformOrigin: '0 0',
              minWidth: '2000px',
              minHeight: '1500px',
              backgroundImage: designerState.gridSnap 
                ? 'radial-gradient(circle, #d1d5db 1px, transparent 1px)'
                : undefined,
              backgroundSize: designerState.gridSnap ? '20px 20px' : undefined
            }}
          >
            {/* SVG for transitions */}
            <svg className="absolute inset-0 w-full h-full pointer-events-none">
              <defs>
                <marker
                  id="arrowhead"
                  markerWidth="10"
                  markerHeight="7"
                  refX="10"
                  refY="3.5"
                  orient="auto"
                >
                  <polygon
                    points="0 0, 10 3.5, 0 7"
                    fill="#6b7280"
                  />
                </marker>
              </defs>
              {renderTransitions()}
            </svg>

            {/* Stages */}
            {designerState.stages.map(stage => (
              <div
                key={stage.id}
                ref={el => el && stageRefs.current.set(stage.id, el)}
                className={`absolute bg-white border-2 rounded-lg shadow-lg cursor-move select-none ${
                  designerState.selectedStage?.id === stage.id 
                    ? 'border-blue-500 ring-2 ring-blue-200' 
                    : 'border-gray-300 hover:border-gray-400'
                } ${designerState.previewMode ? 'cursor-default' : ''}`}
                style={{
                  left: stage.x || 0,
                  top: stage.y || 0,
                  width: stage.width || 180,
                  height: stage.height || 100
                }}
                onMouseDown={(e) => handleStageMouseDown(e, stage)}
                onMouseUp={(e) => handleStageMouseUp(e, stage)}
              >
                <div className="p-3 h-full flex flex-col">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="font-semibold text-sm text-gray-900 truncate">
                      {stage.name}
                    </h3>
                    <div className="flex space-x-1">
                      {stage.approvalRequired && (
                        <span className="text-xs bg-yellow-100 text-yellow-800 px-1 rounded">
                          Approval
                        </span>
                      )}
                      {stage.slaHours && (
                        <span className="text-xs bg-blue-100 text-blue-800 px-1 rounded">
                          SLA: {stage.slaHours}h
                        </span>
                      )}
                    </div>
                  </div>
                  
                  <p className="text-xs text-gray-600 mb-2 flex-1 overflow-hidden">
                    {stage.description || 'No description'}
                  </p>
                  
                  <div className="flex items-center justify-between">
                    <span className="text-xs text-gray-500">
                      Order: {stage.order}
                    </span>
                    
                    {!designerState.previewMode && (
                      <div className="flex space-x-1">
                        {/* Connection point */}
                        <div
                          onMouseDown={(e) => handleConnectorMouseDown(e, stage)}
                          className="w-3 h-3 bg-blue-500 rounded-full cursor-crosshair hover:bg-blue-600"
                          title="Drag to create transition"
                        />
                        
                        {/* Delete button */}
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            deleteStage(stage.id);
                          }}
                          className="text-red-500 hover:text-red-700"
                          title="Delete stage"
                        >
                          ×
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Properties Panel */}
        {(showStageProperties || showTransitionProperties || showValidationPanel) && (
          <div className="w-80 bg-white border-l border-gray-200 flex flex-col">
            {/* Panel tabs */}
            <div className="border-b border-gray-200">
              <nav className="flex space-x-8" aria-label="Tabs">
                {showStageProperties && (
                  <button
                    onClick={() => {
                      setShowStageProperties(true);
                      setShowTransitionProperties(false);
                      setShowValidationPanel(false);
                    }}
                    className={`py-2 px-1 border-b-2 font-medium text-sm ${
                      showStageProperties ? 'border-blue-500 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    Stage Properties
                  </button>
                )}
                {showTransitionProperties && (
                  <button
                    onClick={() => {
                      setShowStageProperties(false);
                      setShowTransitionProperties(true);
                      setShowValidationPanel(false);
                    }}
                    className={`py-2 px-1 border-b-2 font-medium text-sm ${
                      showTransitionProperties ? 'border-blue-500 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    Transition Rules
                  </button>
                )}
                {showValidationPanel && (
                  <button
                    onClick={() => {
                      setShowStageProperties(false);
                      setShowTransitionProperties(false);
                      setShowValidationPanel(true);
                    }}
                    className={`py-2 px-1 border-b-2 font-medium text-sm ${
                      showValidationPanel ? 'border-blue-500 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    Validation
                  </button>
                )}
              </nav>
            </div>

            {/* Panel content */}
            <div className="flex-1 overflow-y-auto p-4">
              {showStageProperties && designerState.selectedStage && (
                <StagePropertiesPanel
                  stage={designerState.selectedStage}
                  onUpdate={updateStage}
                  onClose={() => setShowStageProperties(false)}
                />
              )}
              
              {showTransitionProperties && designerState.selectedTransition && (
                <TransitionPropertiesPanel
                  transition={designerState.selectedTransition}
                  stages={designerState.stages}
                  onUpdate={updateTransition}
                  onClose={() => setShowTransitionProperties(false)}
                />
              )}
              
              {showValidationPanel && (
                <ValidationPanel
                  validationResults={designerState.validationResults}
                  onValidate={validateWorkflow}
                  onClose={() => setShowValidationPanel(false)}
                />
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

// Stage Properties Panel Component
const StagePropertiesPanel: React.FC<{
  stage: WorkflowStageConfiguration;
  onUpdate: (stage: WorkflowStageConfiguration) => void;
  onClose: () => void;
}> = ({ stage, onUpdate, onClose }) => {
  const [localStage, setLocalStage] = useState(stage);

  const handleUpdate = (field: keyof WorkflowStageConfiguration, value: any) => {
    const updatedStage = { ...localStage, [field]: value };
    setLocalStage(updatedStage);
    onUpdate(updatedStage);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-medium text-gray-900">Stage Properties</h3>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          ×
        </button>
      </div>

      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Name</label>
          <input
            type="text"
            value={localStage.name}
            onChange={(e) => handleUpdate('name', e.target.value)}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Description</label>
          <textarea
            value={localStage.description}
            onChange={(e) => handleUpdate('description', e.target.value)}
            rows={3}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Order</label>
          <input
            type="number"
            value={localStage.order}
            onChange={(e) => handleUpdate('order', parseInt(e.target.value))}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Stage Type</label>
          <select
            value={localStage.stageType}
            onChange={(e) => handleUpdate('stageType', e.target.value)}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
          >
            <option value="Standard">Standard</option>
            <option value="Approval">Approval</option>
            <option value="Notification">Notification</option>
            <option value="Auto">Auto</option>
          </select>
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="approvalRequired"
            checked={localStage.approvalRequired}
            onChange={(e) => handleUpdate('approvalRequired', e.target.checked)}
            className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
          <label htmlFor="approvalRequired" className="ml-2 block text-sm text-gray-900">
            Approval Required
          </label>
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="autoTransition"
            checked={localStage.autoTransition}
            onChange={(e) => handleUpdate('autoTransition', e.target.checked)}
            className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
          <label htmlFor="autoTransition" className="ml-2 block text-sm text-gray-900">
            Auto Transition
          </label>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">SLA Hours</label>
          <input
            type="number"
            value={localStage.slaHours || ''}
            onChange={(e) => handleUpdate('slaHours', e.target.value ? parseInt(e.target.value) : undefined)}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="isActive"
            checked={localStage.isActive}
            onChange={(e) => handleUpdate('isActive', e.target.checked)}
            className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
          <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
            Active
          </label>
        </div>
      </div>
    </div>
  );
};

// Transition Properties Panel Component
const TransitionPropertiesPanel: React.FC<{
  transition: WorkflowTransition;
  stages: WorkflowStageConfiguration[];
  onUpdate: (transition: WorkflowTransition) => void;
  onClose: () => void;
}> = ({ transition, stages, onUpdate, onClose }) => {
  const [localTransition, setLocalTransition] = useState(transition);

  const handleUpdate = (field: keyof WorkflowTransition, value: any) => {
    const updatedTransition = { ...localTransition, [field]: value };
    setLocalTransition(updatedTransition);
    onUpdate(updatedTransition);
  };

  const fromStage = stages.find(s => s.id === transition.fromStageId);
  const toStage = stages.find(s => s.id === transition.toStageId);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-medium text-gray-900">Transition Rules</h3>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          ×
        </button>
      </div>

      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Transition Name</label>
          <input
            type="text"
            value={localTransition.transitionName}
            onChange={(e) => handleUpdate('transitionName', e.target.value)}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">From Stage</label>
            <input
              type="text"
              value={fromStage?.name || 'Unknown'}
              disabled
              className="mt-1 block w-full rounded-md border-gray-300 bg-gray-50 shadow-sm"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">To Stage</label>
            <input
              type="text"
              value={toStage?.name || 'Unknown'}
              disabled
              className="mt-1 block w-full rounded-md border-gray-300 bg-gray-50 shadow-sm"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Required Role</label>
          <select
            value={localTransition.requiredRole || ''}
            onChange={(e) => handleUpdate('requiredRole', e.target.value || undefined)}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
          >
            <option value="">Any Role</option>
            <option value={UserRole.EndUser}>End User</option>
            <option value={UserRole.Lead}>Lead</option>
            <option value={UserRole.DepartmentManager}>Department Manager</option>
            <option value={UserRole.DepartmentHead}>Department Head</option>
            <option value={UserRole.BusinessExecutive}>Business Executive</option>
            <option value={UserRole.SystemAdministrator}>System Administrator</option>
          </select>
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="notificationRequired"
            checked={localTransition.notificationRequired}
            onChange={(e) => handleUpdate('notificationRequired', e.target.checked)}
            className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
          <label htmlFor="notificationRequired" className="ml-2 block text-sm text-gray-900">
            Send Notification
          </label>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Auto Transition Delay (minutes)</label>
          <input
            type="number"
            value={localTransition.autoTransitionDelayMinutes || ''}
            onChange={(e) => handleUpdate('autoTransitionDelayMinutes', e.target.value ? parseInt(e.target.value) : undefined)}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="isActive"
            checked={localTransition.isActive}
            onChange={(e) => handleUpdate('isActive', e.target.checked)}
            className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
          <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
            Active
          </label>
        </div>

        {/* Conditional Logic Section */}
        <div className="border-t pt-4">
          <h4 className="text-sm font-medium text-gray-900 mb-2">Conditional Logic</h4>
          <div className="space-y-2">
            <div>
              <label className="block text-sm font-medium text-gray-700">Condition Type</label>
              <select
                value={localTransition.conditionScript.type}
                onChange={(e) => handleUpdate('conditionScript', {
                  ...localTransition.conditionScript,
                  type: e.target.value
                })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              >
                <option value="custom">Custom</option>
                <option value="priority">Priority Based</option>
                <option value="role">Role Based</option>
                <option value="timeElapsed">Time Elapsed</option>
                <option value="businessVertical">Business Vertical</option>
              </select>
            </div>
            <div className="flex items-center">
              <input
                type="checkbox"
                id="conditionActive"
                checked={localTransition.conditionScript.isActive}
                onChange={(e) => handleUpdate('conditionScript', {
                  ...localTransition.conditionScript,
                  isActive: e.target.checked
                })}
                className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <label htmlFor="conditionActive" className="ml-2 block text-sm text-gray-900">
                Enable Conditions
              </label>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// Validation Panel Component
const ValidationPanel: React.FC<{
  validationResults: WorkflowValidationResult;
  onValidate: () => void;
  onClose: () => void;
}> = ({ validationResults, onValidate, onClose }) => {
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-medium text-gray-900">Workflow Validation</h3>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          ×
        </button>
      </div>

      <button
        onClick={onValidate}
        className="w-full px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        Validate Workflow
      </button>

      <div className="space-y-4">
        <div className={`p-4 rounded-md ${
          validationResults.isValid ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'
        }`}>
          <div className="flex">
            <div className="flex-shrink-0">
              {validationResults.isValid ? (
                <span className="text-green-500">✓</span>
              ) : (
                <span className="text-red-500">✗</span>
              )}
            </div>
            <div className="ml-3">
              <h3 className={`text-sm font-medium ${
                validationResults.isValid ? 'text-green-800' : 'text-red-800'
              }`}>
                {validationResults.isValid ? 'Workflow is valid' : 'Workflow has validation errors'}
              </h3>
            </div>
          </div>
        </div>

        {validationResults.validationErrors.length > 0 && (
          <div>
            <h4 className="text-sm font-medium text-red-800 mb-2">Errors:</h4>
            <ul className="space-y-1">
              {validationResults.validationErrors.map((error, index) => (
                <li key={index} className="text-sm text-red-700 bg-red-50 p-2 rounded">
                  {error}
                </li>
              ))}
            </ul>
          </div>
        )}

        {validationResults.warnings.length > 0 && (
          <div>
            <h4 className="text-sm font-medium text-yellow-800 mb-2">Warnings:</h4>
            <ul className="space-y-1">
              {validationResults.warnings.map((warning, index) => (
                <li key={index} className="text-sm text-yellow-700 bg-yellow-50 p-2 rounded">
                  {warning}
                </li>
              ))}
            </ul>
          </div>
        )}

        <div className="text-xs text-gray-500">
          Last validated: {new Date(validationResults.validationDate).toLocaleString()}
        </div>
      </div>
    </div>
  );
};

export default WorkflowDesigner; 