import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  Alert,
  CircularProgress,
  Card,
  CardContent,
  Stepper,
  Step,
  StepLabel,
  FormHelperText
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';
import apiService from '../services/api';
import { 
  WorkRequest, 
  type CreateWorkRequest, 
  WorkCategory, 
  Department, 
  BusinessVertical,
  PriorityLevel 
} from '../types';

const steps = ['Basic Information', 'Business Details', 'Resource Requirements', 'Review & Submit'];

const CreateWorkRequestPage: React.FC = () => {
  const [activeStep, setActiveStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  const [departments, setDepartments] = useState<Department[]>([]);
  const [businessVerticals, setBusinessVerticals] = useState<BusinessVertical[]>([]);
  
  // Using the singleton instance from the imported apiService

  // Form state
  const [formData, setFormData] = useState<CreateWorkRequest>({
    title: '',
    description: '',
    category: WorkCategory.WorkRequest,
    businessVerticalId: 0,
    departmentId: 0,
    targetDate: undefined,
    capabilityId: undefined,
    estimatedEffort: 0,
    businessValue: 0.5
  });

  // Form validation
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    loadInitialData();
  }, []);

  const loadInitialData = async () => {
    try {
      setLoading(true);
      const [deptsData, verticalsData] = await Promise.all([
        apiService.getDepartments(),
        apiService.getBusinessVerticals()
      ]);
      setDepartments(deptsData);
      setBusinessVerticals(verticalsData);
    } catch (err) {
      setError('Failed to load initial data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field: keyof CreateWorkRequest, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error for this field
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const validateStep = (step: number): boolean => {
    const newErrors: Record<string, string> = {};

    switch (step) {
      case 0: // Basic Information
        if (!formData.title.trim()) newErrors.title = 'Title is required';
        if (!formData.description.trim()) newErrors.description = 'Description is required';
        if (formData.description.length < 10) newErrors.description = 'Description must be at least 10 characters';
        break;
      
      case 1: // Business Details
        if (!formData.businessVerticalId) newErrors.businessVerticalId = 'Business Vertical is required';
        if (!formData.departmentId) newErrors.departmentId = 'Department is required';
        if (formData.businessValue < 0 || formData.businessValue > 1) {
          newErrors.businessValue = 'Business Value must be between 0 and 1';
        }
        break;
      
      case 2: // Resource Requirements
        if (formData.estimatedEffort <= 0) newErrors.estimatedEffort = 'Estimated effort must be greater than 0';
        if (formData.estimatedEffort > 1000) newErrors.estimatedEffort = 'Estimated effort seems too high';
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleNext = () => {
    if (validateStep(activeStep)) {
      setActiveStep(prev => prev + 1);
    }
  };

  const handleBack = () => {
    setActiveStep(prev => prev - 1);
  };

  const handleSubmit = async () => {
    if (!validateStep(activeStep)) return;

    try {
      setLoading(true);
      setError(null);
      
      const workRequest = await apiService.createWorkRequest(formData);
      
      setSuccess(`Work request "${workRequest.title}" created successfully!`);
      
      // Reset form
      setFormData({
        title: '',
        description: '',
        category: WorkCategory.WorkRequest,
        businessVerticalId: 0,
        departmentId: 0,
        targetDate: undefined,
        capabilityId: undefined,
        estimatedEffort: 0,
        businessValue: 0.5
      });
      setActiveStep(0);
      
    } catch (err) {
      setError('Failed to create work request');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const getStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Title"
                value={formData.title}
                onChange={(e) => handleInputChange('title', e.target.value)}
                error={!!errors.title}
                helperText={errors.title}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                value={formData.description}
                onChange={(e) => handleInputChange('description', e.target.value)}
                multiline
                rows={4}
                error={!!errors.description}
                helperText={errors.description}
                required
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Category</InputLabel>
                <Select
                  value={formData.category}
                  onChange={(e) => handleInputChange('category', e.target.value)}
                  label="Category"
                >
                  {Object.entries(WorkCategory)
                    .filter(([key]) => isNaN(Number(key)))
                    .map(([key, value]) => (
                      <MenuItem key={value} value={value}>
                        {key.replace(/([A-Z])/g, ' $1').trim()}
                      </MenuItem>
                    ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        );

      case 1:
        return (
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth error={!!errors.businessVerticalId}>
                <InputLabel>Business Vertical</InputLabel>
                <Select
                  value={formData.businessVerticalId}
                  onChange={(e) => handleInputChange('businessVerticalId', e.target.value)}
                  label="Business Vertical"
                  required
                >
                  {businessVerticals.map(vertical => (
                    <MenuItem key={vertical.id} value={vertical.id}>
                      {vertical.name}
                    </MenuItem>
                  ))}
                </Select>
                {errors.businessVerticalId && (
                  <FormHelperText>{errors.businessVerticalId}</FormHelperText>
                )}
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth error={!!errors.departmentId}>
                <InputLabel>Department</InputLabel>
                <Select
                  value={formData.departmentId}
                  onChange={(e) => handleInputChange('departmentId', e.target.value)}
                  label="Department"
                  required
                >
                  {departments
                    .filter(dept => !formData.businessVerticalId || dept.businessVerticalId === formData.businessVerticalId)
                    .map(dept => (
                      <MenuItem key={dept.id} value={dept.id}>
                        {dept.name}
                      </MenuItem>
                    ))}
                </Select>
                {errors.departmentId && (
                  <FormHelperText>{errors.departmentId}</FormHelperText>
                )}
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <LocalizationProvider dateAdapter={AdapterDayjs}>
                <DatePicker
                  label="Target Date"
                  value={formData.targetDate ? dayjs(formData.targetDate) : null}
                  onChange={(date) => handleInputChange('targetDate', date?.toISOString())}
                  slotProps={{
                    textField: {
                      fullWidth: true,
                      helperText: 'Optional target completion date'
                    }
                  }}
                />
              </LocalizationProvider>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Business Value Score"
                type="number"
                inputProps={{ min: 0, max: 1, step: 0.1 }}
                value={formData.businessValue}
                onChange={(e) => handleInputChange('businessValue', parseFloat(e.target.value))}
                error={!!errors.businessValue}
                helperText={errors.businessValue || 'Score from 0 (low) to 1 (high)'}
              />
            </Grid>
          </Grid>
        );

      case 2:
        return (
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Estimated Effort (hours)"
                type="number"
                inputProps={{ min: 1, max: 1000 }}
                value={formData.estimatedEffort}
                onChange={(e) => handleInputChange('estimatedEffort', parseInt(e.target.value))}
                error={!!errors.estimatedEffort}
                helperText={errors.estimatedEffort || 'Estimated hours to complete'}
                required
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Capability (Optional)</InputLabel>
                <Select
                  value={formData.capabilityId || ''}
                  onChange={(e) => handleInputChange('capabilityId', e.target.value || undefined)}
                  label="Capability (Optional)"
                >
                  <MenuItem value="">None</MenuItem>
                  {/* Capabilities would be loaded here */}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        );

      case 3:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Review Your Work Request
            </Typography>
            <Card>
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Title
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {formData.title}
                    </Typography>
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Description
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {formData.description}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Category
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {Object.keys(WorkCategory).find(key => WorkCategory[key as keyof typeof WorkCategory] === formData.category)}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Business Vertical
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {businessVerticals.find(v => v.id === formData.businessVerticalId)?.name}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Department
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {departments.find(d => d.id === formData.departmentId)?.name}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Estimated Effort
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {formData.estimatedEffort} hours
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Business Value Score
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {(formData.businessValue * 100).toFixed(0)}%
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="textSecondary">
                      Target Date
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {formData.targetDate ? new Date(formData.targetDate).toLocaleDateString() : 'Not specified'}
                    </Typography>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Box>
        );

      default:
        return 'Unknown step';
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Create Work Request
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}

      <Paper sx={{ p: 3 }}>
        <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        {loading ? (
          <Box display="flex" justifyContent="center" p={3}>
            <CircularProgress />
          </Box>
        ) : (
          <>
            <Box sx={{ mb: 3 }}>
              {getStepContent(activeStep)}
            </Box>

            <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
              <Button
                disabled={activeStep === 0}
                onClick={handleBack}
              >
                Back
              </Button>
              
              <Box>
                {activeStep === steps.length - 1 ? (
                  <Button
                    variant="contained"
                    onClick={handleSubmit}
                    disabled={loading}
                  >
                    {loading ? <CircularProgress size={20} /> : 'Create Work Request'}
                  </Button>
                ) : (
                  <Button
                    variant="contained"
                    onClick={handleNext}
                  >
                    Next
                  </Button>
                )}
              </Box>
            </Box>
          </>
        )}
      </Paper>
    </Box>
  );
};

export default CreateWorkRequestPage;