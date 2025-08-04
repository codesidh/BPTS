import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  IconButton,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Switch,
  FormControlLabel,
  Divider,
  Tooltip,
  Badge,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Drawer,
  AppBar,
  Toolbar,
  Fab,
  SpeedDial,
  SpeedDialAction,
  SpeedDialIcon
} from '@mui/material';
import {
  Category,
  Add,
  Edit,
  Delete,
  Visibility,
  Settings,
  Save,
  Cancel,
  Close,
  ExpandMore,
  CheckCircle,
  Warning,
  Error,
  Refresh,
  FilterList,
  Search,
  FormBuilder,
  Approval,
  Security,
  Business,
  Description,
  Validation,
  Notifications,
  Customize
} from '@mui/icons-material';
import apiService from '../services/api';

interface WorkCategory {
  id: number;
  categoryName: string;
  description: string;
  businessVerticalId?: number;
  businessVerticalName?: string;
  requiredFields: string;
  approvalMatrix: string;
  validationRules: string;
  notificationTemplates: string;
  customFields: string;
  isActive: boolean;
  createdDate: string;
  modifiedDate: string;
  createdBy: string;
  modifiedBy: string;
}

interface BusinessVertical {
  id: number;
  name: string;
  description: string;
}

interface FormField {
  name: string;
  type: string;
  label: string;
  required: boolean;
  validation?: string;
  options?: string[];
}

interface ApprovalMatrix {
  levels: ApprovalLevel[];
}

interface ApprovalLevel {
  level: number;
  role: string;
  department?: string;
  businessVertical?: string;
  autoApprove: boolean;
}

const WorkCategoryManagement: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [categories, setCategories] = useState<WorkCategory[]>([]);
  const [filteredCategories, setFilteredCategories] = useState<WorkCategory[]>([]);
  const [businessVerticals, setBusinessVerticals] = useState<BusinessVertical[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  // Dialog states
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<WorkCategory | null>(null);
  const [formBuilderOpen, setFormBuilderOpen] = useState(false);
  const [approvalMatrixOpen, setApprovalMatrixOpen] = useState(false);
  const [validationRulesOpen, setValidationRulesOpen] = useState(false);
  const [notificationTemplatesOpen, setNotificationTemplatesOpen] = useState(false);
  
  // Form states
  const [categoryName, setCategoryName] = useState('');
  const [description, setDescription] = useState('');
  const [businessVerticalId, setBusinessVerticalId] = useState<number | ''>('');
  const [isActive, setIsActive] = useState(true);
  const [requiredFields, setRequiredFields] = useState<FormField[]>([]);
  const [approvalMatrix, setApprovalMatrix] = useState<ApprovalMatrix>({ levels: [] });
  const [validationRules, setValidationRules] = useState<string>('');
  const [notificationTemplates, setNotificationTemplates] = useState<string>('');
  const [customFields, setCustomFields] = useState<string>('');
  
  // Search and filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [verticalFilter, setVerticalFilter] = useState<string>('all');

  useEffect(() => {
    loadCategories();
    loadBusinessVerticals();
  }, []);

  useEffect(() => {
    filterCategories();
  }, [categories, searchTerm, statusFilter, verticalFilter]);

  const loadCategories = async () => {
    try {
      setLoading(true);
      const response = await apiService.get('/api/workcategories');
      setCategories(response.data);
    } catch (err) {
      setError('Failed to load work categories');
      console.error('Error loading categories:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadBusinessVerticals = async () => {
    try {
      const response = await apiService.get('/api/businessverticals');
      setBusinessVerticals(response.data);
    } catch (err) {
      console.error('Error loading business verticals:', err);
    }
  };

  const filterCategories = () => {
    let filtered = categories;

    // Apply search filter
    if (searchTerm) {
      filtered = filtered.filter(cat => 
        cat.categoryName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        cat.description.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Apply status filter
    if (statusFilter !== 'all') {
      filtered = filtered.filter(cat => 
        statusFilter === 'active' ? cat.isActive : !cat.isActive
      );
    }

    // Apply vertical filter
    if (verticalFilter !== 'all') {
      filtered = filtered.filter(cat => 
        cat.businessVerticalId?.toString() === verticalFilter
      );
    }

    setFilteredCategories(filtered);
  };

  const handleAddCategory = () => {
    setEditingCategory(null);
    resetForm();
    setDialogOpen(true);
  };

  const handleEditCategory = (category: WorkCategory) => {
    setEditingCategory(category);
    setCategoryName(category.categoryName);
    setDescription(category.description);
    setBusinessVerticalId(category.businessVerticalId || '');
    setIsActive(category.isActive);
    
    // Parse JSON fields
    try {
      if (category.requiredFields) {
        setRequiredFields(JSON.parse(category.requiredFields));
      }
      if (category.approvalMatrix) {
        setApprovalMatrix(JSON.parse(category.approvalMatrix));
      }
      setValidationRules(category.validationRules || '');
      setNotificationTemplates(category.notificationTemplates || '');
      setCustomFields(category.customFields || '');
    } catch (err) {
      console.error('Error parsing category data:', err);
    }
    
    setDialogOpen(true);
  };

  const handleSaveCategory = async () => {
    try {
      setLoading(true);
      const categoryData = {
        categoryName,
        description,
        businessVerticalId: businessVerticalId || null,
        isActive,
        requiredFields: JSON.stringify(requiredFields),
        approvalMatrix: JSON.stringify(approvalMatrix),
        validationRules,
        notificationTemplates,
        customFields
      };

      if (editingCategory) {
        await apiService.put(`/api/workcategories/${editingCategory.id}`, categoryData);
        setSuccess('Work category updated successfully');
      } else {
        await apiService.post('/api/workcategories', categoryData);
        setSuccess('Work category created successfully');
      }

      setDialogOpen(false);
      loadCategories();
      resetForm();
    } catch (err) {
      setError('Failed to save work category');
      console.error('Error saving category:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteCategory = async (categoryId: number) => {
    if (window.confirm('Are you sure you want to delete this work category?')) {
      try {
        setLoading(true);
        await apiService.delete(`/api/workcategories/${categoryId}`);
        setSuccess('Work category deleted successfully');
        loadCategories();
      } catch (err) {
        setError('Failed to delete work category');
        console.error('Error deleting category:', err);
      } finally {
        setLoading(false);
      }
    }
  };

  const resetForm = () => {
    setCategoryName('');
    setDescription('');
    setBusinessVerticalId('');
    setIsActive(true);
    setRequiredFields([]);
    setApprovalMatrix({ levels: [] });
    setValidationRules('');
    setNotificationTemplates('');
    setCustomFields('');
  };

  const addFormField = () => {
    const newField: FormField = {
      name: '',
      type: 'text',
      label: '',
      required: false
    };
    setRequiredFields([...requiredFields, newField]);
  };

  const updateFormField = (index: number, field: FormField) => {
    const updatedFields = [...requiredFields];
    updatedFields[index] = field;
    setRequiredFields(updatedFields);
  };

  const removeFormField = (index: number) => {
    setRequiredFields(requiredFields.filter((_, i) => i !== index));
  };

  const addApprovalLevel = () => {
    const newLevel: ApprovalLevel = {
      level: approvalMatrix.levels.length + 1,
      role: '',
      autoApprove: false
    };
    setApprovalMatrix({
      levels: [...approvalMatrix.levels, newLevel]
    });
  };

  const updateApprovalLevel = (index: number, level: ApprovalLevel) => {
    const updatedLevels = [...approvalMatrix.levels];
    updatedLevels[index] = level;
    setApprovalMatrix({ levels: updatedLevels });
  };

  const removeApprovalLevel = (index: number) => {
    setApprovalMatrix({
      levels: approvalMatrix.levels.filter((_, i) => i !== index)
    });
  };

  const getStatusChip = (category: WorkCategory) => {
    return category.isActive ? (
      <Chip label="Active" color="success" size="small" />
    ) : (
      <Chip label="Inactive" color="default" size="small" />
    );
  };

  const renderFormBuilder = () => (
    <Dialog open={formBuilderOpen} onClose={() => setFormBuilderOpen(false)} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box display="flex" alignItems="center">
          <FormBuilder sx={{ mr: 1 }} />
          Form Builder
        </Box>
      </DialogTitle>
      <DialogContent>
        <Typography variant="h6" gutterBottom>Required Fields</Typography>
        <List>
          {requiredFields.map((field, index) => (
            <ListItem key={index}>
              <Grid container spacing={2} alignItems="center">
                <Grid item xs={3}>
                  <TextField
                    label="Field Name"
                    value={field.name}
                    onChange={(e) => updateFormField(index, { ...field, name: e.target.value })}
                    fullWidth
                    size="small"
                  />
                </Grid>
                <Grid item xs={3}>
                  <FormControl fullWidth size="small">
                    <InputLabel>Type</InputLabel>
                    <Select
                      value={field.type}
                      onChange={(e) => updateFormField(index, { ...field, type: e.target.value })}
                    >
                      <MenuItem value="text">Text</MenuItem>
                      <MenuItem value="number">Number</MenuItem>
                      <MenuItem value="email">Email</MenuItem>
                      <MenuItem value="date">Date</MenuItem>
                      <MenuItem value="select">Select</MenuItem>
                      <MenuItem value="textarea">Text Area</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={3}>
                  <TextField
                    label="Label"
                    value={field.label}
                    onChange={(e) => updateFormField(index, { ...field, label: e.target.value })}
                    fullWidth
                    size="small"
                  />
                </Grid>
                <Grid item xs={2}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={field.required}
                        onChange={(e) => updateFormField(index, { ...field, required: e.target.checked })}
                      />
                    }
                    label="Required"
                  />
                </Grid>
                <Grid item xs={1}>
                  <IconButton onClick={() => removeFormField(index)} color="error">
                    <Delete />
                  </IconButton>
                </Grid>
              </Grid>
            </ListItem>
          ))}
        </List>
        <Button
          startIcon={<Add />}
          onClick={addFormField}
          variant="outlined"
          sx={{ mt: 2 }}
        >
          Add Field
        </Button>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => setFormBuilderOpen(false)}>Close</Button>
      </DialogActions>
    </Dialog>
  );

  const renderApprovalMatrix = () => (
    <Dialog open={approvalMatrixOpen} onClose={() => setApprovalMatrixOpen(false)} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box display="flex" alignItems="center">
          <Approval sx={{ mr: 1 }} />
          Approval Matrix
        </Box>
      </DialogTitle>
      <DialogContent>
        <Typography variant="h6" gutterBottom>Approval Levels</Typography>
        <List>
          {approvalMatrix.levels.map((level, index) => (
            <ListItem key={index}>
              <Grid container spacing={2} alignItems="center">
                <Grid item xs={2}>
                  <TextField
                    label="Level"
                    value={level.level}
                    onChange={(e) => updateApprovalLevel(index, { ...level, level: parseInt(e.target.value) })}
                    type="number"
                    size="small"
                  />
                </Grid>
                <Grid item xs={3}>
                  <TextField
                    label="Role"
                    value={level.role}
                    onChange={(e) => updateApprovalLevel(index, { ...level, role: e.target.value })}
                    fullWidth
                    size="small"
                  />
                </Grid>
                <Grid item xs={3}>
                  <FormControl fullWidth size="small">
                    <InputLabel>Department</InputLabel>
                    <Select
                      value={level.department || ''}
                      onChange={(e) => updateApprovalLevel(index, { ...level, department: e.target.value })}
                    >
                      <MenuItem value="">Any</MenuItem>
                      <MenuItem value="IT">IT</MenuItem>
                      <MenuItem value="Finance">Finance</MenuItem>
                      <MenuItem value="HR">HR</MenuItem>
                      <MenuItem value="Operations">Operations</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={2}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={level.autoApprove}
                        onChange={(e) => updateApprovalLevel(index, { ...level, autoApprove: e.target.checked })}
                      />
                    }
                    label="Auto Approve"
                  />
                </Grid>
                <Grid item xs={1}>
                  <IconButton onClick={() => removeApprovalLevel(index)} color="error">
                    <Delete />
                  </IconButton>
                </Grid>
              </Grid>
            </ListItem>
          ))}
        </List>
        <Button
          startIcon={<Add />}
          onClick={addApprovalLevel}
          variant="outlined"
          sx={{ mt: 2 }}
        >
          Add Level
        </Button>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => setApprovalMatrixOpen(false)}>Close</Button>
      </DialogActions>
    </Dialog>
  );

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Work Category Management
      </Typography>

      {error && (
        <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" onClose={() => setSuccess(null)} sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      <Paper sx={{ mb: 3 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)}>
            <Tab label="Categories" />
            <Tab label="Form Builder" />
            <Tab label="Approval Matrix" />
            <Tab label="Validation Rules" />
            <Tab label="Notifications" />
          </Tabs>
        </Box>

        <Box sx={{ p: 2 }}>
          {activeTab === 0 && (
            <Box>
              <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                <Typography variant="h6">Work Categories</Typography>
                <Button
                  variant="contained"
                  startIcon={<Add />}
                  onClick={handleAddCategory}
                >
                  Add Category
                </Button>
              </Box>

              <Grid container spacing={2} sx={{ mb: 2 }}>
                <Grid item xs={12} md={4}>
                  <TextField
                    fullWidth
                    label="Search categories"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    InputProps={{
                      startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} />
                    }}
                  />
                </Grid>
                <Grid item xs={12} md={3}>
                  <FormControl fullWidth>
                    <InputLabel>Status</InputLabel>
                    <Select
                      value={statusFilter}
                      onChange={(e) => setStatusFilter(e.target.value)}
                    >
                      <MenuItem value="all">All</MenuItem>
                      <MenuItem value="active">Active</MenuItem>
                      <MenuItem value="inactive">Inactive</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} md={3}>
                  <FormControl fullWidth>
                    <InputLabel>Business Vertical</InputLabel>
                    <Select
                      value={verticalFilter}
                      onChange={(e) => setVerticalFilter(e.target.value)}
                    >
                      <MenuItem value="all">All</MenuItem>
                      {businessVerticals.map(vertical => (
                        <MenuItem key={vertical.id} value={vertical.id.toString()}>
                          {vertical.name}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} md={2}>
                  <Button
                    fullWidth
                    variant="outlined"
                    startIcon={<Refresh />}
                    onClick={loadCategories}
                  >
                    Refresh
                  </Button>
                </Grid>
              </Grid>

              {loading ? (
                <Box display="flex" justifyContent="center" p={3}>
                  <CircularProgress />
                </Box>
              ) : (
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Category Name</TableCell>
                        <TableCell>Description</TableCell>
                        <TableCell>Business Vertical</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell>Created</TableCell>
                        <TableCell>Actions</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {filteredCategories.map((category) => (
                        <TableRow key={category.id}>
                          <TableCell>
                            <Typography variant="subtitle2">
                              {category.categoryName}
                            </Typography>
                          </TableCell>
                          <TableCell>{category.description}</TableCell>
                          <TableCell>{category.businessVerticalName || 'N/A'}</TableCell>
                          <TableCell>{getStatusChip(category)}</TableCell>
                          <TableCell>
                            {new Date(category.createdDate).toLocaleDateString()}
                          </TableCell>
                          <TableCell>
                            <Box display="flex" gap={1}>
                              <Tooltip title="View Details">
                                <IconButton size="small">
                                  <Visibility />
                                </IconButton>
                              </Tooltip>
                              <Tooltip title="Edit Category">
                                <IconButton 
                                  size="small" 
                                  onClick={() => handleEditCategory(category)}
                                >
                                  <Edit />
                                </IconButton>
                              </Tooltip>
                              <Tooltip title="Delete Category">
                                <IconButton 
                                  size="small" 
                                  color="error"
                                  onClick={() => handleDeleteCategory(category.id)}
                                >
                                  <Delete />
                                </IconButton>
                              </Tooltip>
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </Box>
          )}

          {activeTab === 1 && (
            <Box>
              <Typography variant="h6" gutterBottom>Form Builder</Typography>
              <Typography color="text.secondary">
                Configure dynamic forms for work categories. Select a category to edit its form fields.
              </Typography>
            </Box>
          )}

          {activeTab === 2 && (
            <Box>
              <Typography variant="h6" gutterBottom>Approval Matrix</Typography>
              <Typography color="text.secondary">
                Configure approval workflows for work categories. Select a category to edit its approval matrix.
              </Typography>
            </Box>
          )}

          {activeTab === 3 && (
            <Box>
              <Typography variant="h6" gutterBottom>Validation Rules</Typography>
              <Typography color="text.secondary">
                Configure validation rules for work categories. Select a category to edit its validation rules.
              </Typography>
            </Box>
          )}

          {activeTab === 4 && (
            <Box>
              <Typography variant="h6" gutterBottom>Notification Templates</Typography>
              <Typography color="text.secondary">
                Configure notification templates for work categories. Select a category to edit its notification templates.
              </Typography>
            </Box>
          )}
        </Box>
      </Paper>

      {/* Add/Edit Category Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingCategory ? 'Edit Work Category' : 'Add Work Category'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Category Name"
                value={categoryName}
                onChange={(e) => setCategoryName(e.target.value)}
                required
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Business Vertical</InputLabel>
                <Select
                  value={businessVerticalId}
                  onChange={(e) => setBusinessVerticalId(e.target.value as number)}
                >
                  <MenuItem value="">None</MenuItem>
                  {businessVerticals.map(vertical => (
                    <MenuItem key={vertical.id} value={vertical.id}>
                      {vertical.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                multiline
                rows={3}
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={isActive}
                    onChange={(e) => setIsActive(e.target.checked)}
                  />
                }
                label="Active"
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 2 }} />

          <Typography variant="h6" gutterBottom>Configuration</Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={3}>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<FormBuilder />}
                onClick={() => setFormBuilderOpen(true)}
              >
                Form Fields
              </Button>
            </Grid>
            <Grid item xs={12} md={3}>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<Approval />}
                onClick={() => setApprovalMatrixOpen(true)}
              >
                Approval Matrix
              </Button>
            </Grid>
            <Grid item xs={12} md={3}>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<Validation />}
                onClick={() => setValidationRulesOpen(true)}
              >
                Validation Rules
              </Button>
            </Grid>
            <Grid item xs={12} md={3}>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<Notifications />}
                onClick={() => setNotificationTemplatesOpen(true)}
              >
                Notifications
              </Button>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button 
            onClick={handleSaveCategory} 
            variant="contained"
            disabled={loading || !categoryName}
          >
            {loading ? <CircularProgress size={20} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {renderFormBuilder()}
      {renderApprovalMatrix()}
    </Box>
  );
};

export default WorkCategoryManagement; 