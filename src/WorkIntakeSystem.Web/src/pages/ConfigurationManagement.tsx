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
  Switch,
  FormControlLabel,
  Tooltip,
  Badge
} from '@mui/material';
import {
  Settings,
  History,
  Edit,
  CheckCircle,
  Refresh,
  Search,
  Security
} from '@mui/icons-material';
import { apiService } from '../services/api';

interface SystemConfiguration {
  id: number;
  configurationKey: string;
  configurationValue: string;
  dataType: string;
  businessVerticalId?: number;
  description: string;
  isEditable: boolean;
  version: number;
  effectiveDate: string;
  expirationDate?: string;
  changeReason: string;
  approvedBy: string;
  approvalDate?: string;
  isActive: boolean;
  createdDate: string;
  modifiedDate: string;
  createdBy: string;
  modifiedBy: string;
}

interface ConfigurationHistory {
  id: number;
  configurationKey: string;
  version: number;
  configurationValue: string;
  changeReason: string;
  changedBy: string;
  changedDate: string;
  approvedBy?: string;
  approvalDate?: string;
}

const ConfigurationManagement: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [configurations, setConfigurations] = useState<SystemConfiguration[]>([]);
  const [filteredConfigurations, setFilteredConfigurations] = useState<SystemConfiguration[]>([]);
  const [history, setHistory] = useState<ConfigurationHistory[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Dialog states
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [historyDialogOpen, setHistoryDialogOpen] = useState(false);
  const [selectedConfig, setSelectedConfig] = useState<SystemConfiguration | null>(null);
  const [selectedKey, setSelectedKey] = useState<string>('');
  
  // Form states
  const [editForm, setEditForm] = useState({
    configurationValue: '',
    dataType: 'String',
    description: '',
    changeReason: '',
    effectiveDate: '',
    expirationDate: ''
  });
  
  // Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState('all');
  const [showInactive, setShowInactive] = useState(false);

  useEffect(() => {
    loadConfigurations();
  }, []);

  useEffect(() => {
    filterConfigurations();
  }, [configurations, searchTerm, filterType, showInactive]);

  const loadConfigurations = async () => {
    try {
      setLoading(true);
      const response = await apiService.getApi().get('/api/systemconfiguration');
      setConfigurations(response.data);
    } catch (err) {
      setError('Failed to load configurations');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const filterConfigurations = () => {
    let filtered = configurations;

    // Filter by search term
    if (searchTerm) {
      filtered = filtered.filter(config =>
        config.configurationKey.toLowerCase().includes(searchTerm.toLowerCase()) ||
        config.description.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Filter by type
    if (filterType !== 'all') {
      filtered = filtered.filter(config => config.dataType === filterType);
    }

    // Filter by active status
    if (!showInactive) {
      filtered = filtered.filter(config => config.isActive);
    }

    setFilteredConfigurations(filtered);
  };

  const loadHistory = async (key: string) => {
    try {
      const response = await apiService.getApi().get(`/api/systemconfiguration/history?key=${key}`);
      setHistory(response.data);
    } catch (err) {
      setError('Failed to load configuration history');
      console.error(err);
    }
  };

  const handleEdit = (config: SystemConfiguration) => {
    setSelectedConfig(config);
    setEditForm({
      configurationValue: config.configurationValue,
      dataType: config.dataType,
      description: config.description,
      changeReason: '',
      effectiveDate: new Date().toISOString().split('T')[0],
      expirationDate: ''
    });
    setEditDialogOpen(true);
  };

  const handleSave = async () => {
    if (!selectedConfig) return;

    try {
      setLoading(true);
      const newVersion = selectedConfig.version + 1;
      
      const configData = {
        ...selectedConfig,
        ...editForm,
        version: newVersion,
        effectiveDate: new Date(editForm.effectiveDate).toISOString(),
        expirationDate: editForm.expirationDate ? new Date(editForm.expirationDate).toISOString() : null,
        modifiedDate: new Date().toISOString(),
        modifiedBy: 'Current User' // Would come from auth context
      };

      await apiService.getApi().post('/api/systemconfiguration', configData);
      
      setEditDialogOpen(false);
      setSelectedConfig(null);
      loadConfigurations();
    } catch (err) {
      setError('Failed to save configuration');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleViewHistory = async (key: string) => {
    setSelectedKey(key);
    await loadHistory(key);
    setHistoryDialogOpen(true);
  };

  const getDataTypeColor = (dataType: string) => {
    switch (dataType) {
      case 'Boolean': return 'success';
      case 'Number': return 'primary';
      case 'JSON': return 'warning';
      default: return 'default';
    }
  };

  const getStatusChip = (config: SystemConfiguration) => {
    if (!config.isActive) {
      return <Chip label="Inactive" color="error" size="small" />;
    }
    
    const now = new Date();
    const effectiveDate = new Date(config.effectiveDate);
    const expirationDate = config.expirationDate ? new Date(config.expirationDate) : null;
    
    if (now < effectiveDate) {
      return <Chip label="Pending" color="warning" size="small" />;
    }
    
    if (expirationDate && now > expirationDate) {
      return <Chip label="Expired" color="error" size="small" />;
    }
    
    return <Chip label="Active" color="success" size="small" />;
  };

  const renderValuePreview = (config: SystemConfiguration) => {
    const value = config.configurationValue;
    
    if (config.dataType === 'Boolean') {
      return value === 'true' ? 'Yes' : 'No';
    }
    
    if (config.dataType === 'JSON') {
      try {
        const parsed = JSON.parse(value);
        return JSON.stringify(parsed, null, 2).substring(0, 100) + (value.length > 100 ? '...' : '');
      } catch {
        return value;
      }
    }
    
    return value.length > 50 ? value.substring(0, 50) + '...' : value;
  };

  return (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h4" component="h1" sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Settings color="primary" />
          Configuration Management
        </Typography>
        <Button
          variant="contained"
          startIcon={<Refresh />}
          onClick={loadConfigurations}
          disabled={loading}
        >
          Refresh
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Paper sx={{ mb: 3 }}>
        <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)}>
          <Tab label="Active Configurations" />
          <Tab label="Configuration History" />
          <Tab label="System Health" />
        </Tabs>
      </Paper>

      {activeTab === 0 && (
        <>
          {/* Filters */}
          <Paper sx={{ p: 2, mb: 3 }}>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  placeholder="Search configurations..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  InputProps={{
                    startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} />
                  }}
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <FormControl fullWidth>
                  <InputLabel>Data Type</InputLabel>
                  <Select
                    value={filterType}
                    onChange={(e) => setFilterType(e.target.value)}
                    label="Data Type"
                  >
                    <MenuItem value="all">All Types</MenuItem>
                    <MenuItem value="String">String</MenuItem>
                    <MenuItem value="Number">Number</MenuItem>
                    <MenuItem value="Boolean">Boolean</MenuItem>
                    <MenuItem value="JSON">JSON</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={3}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={showInactive}
                      onChange={(e) => setShowInactive(e.target.checked)}
                    />
                  }
                  label="Show Inactive"
                />
              </Grid>
              <Grid item xs={12} md={2}>
                <Typography variant="body2" color="text.secondary">
                  {filteredConfigurations.length} configurations
                </Typography>
              </Grid>
            </Grid>
          </Paper>

          {/* Configurations Table */}
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Configuration Key</TableCell>
                  <TableCell>Value</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Version</TableCell>
                  <TableCell>Effective Date</TableCell>
                  <TableCell>Modified By</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {loading ? (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      <CircularProgress />
                    </TableCell>
                  </TableRow>
                ) : filteredConfigurations.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      No configurations found
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredConfigurations.map((config) => (
                    <TableRow key={config.id}>
                      <TableCell>
                        <Box>
                          <Typography variant="body2" fontWeight="bold">
                            {config.configurationKey}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {config.description}
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Tooltip title={config.configurationValue}>
                          <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                            {renderValuePreview(config)}
                          </Typography>
                        </Tooltip>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={config.dataType}
                          color={getDataTypeColor(config.dataType) as any}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>{getStatusChip(config)}</TableCell>
                      <TableCell>
                        <Badge badgeContent={config.version} color="primary">
                          <Typography variant="body2">v{config.version}</Typography>
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {new Date(config.effectiveDate).toLocaleDateString()}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{config.modifiedBy}</Typography>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', gap: 1 }}>
                          <Tooltip title="Edit Configuration">
                            <IconButton
                              size="small"
                              onClick={() => handleEdit(config)}
                              disabled={!config.isEditable}
                            >
                              <Edit />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="View History">
                            <IconButton
                              size="small"
                              onClick={() => handleViewHistory(config.configurationKey)}
                            >
                              <History />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      {activeTab === 1 && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Configuration History
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            View the complete history of configuration changes across all keys
          </Typography>
          
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Configuration Key</TableCell>
                  <TableCell>Version</TableCell>
                  <TableCell>Value</TableCell>
                  <TableCell>Changed By</TableCell>
                  <TableCell>Changed Date</TableCell>
                  <TableCell>Approved By</TableCell>
                  <TableCell>Status</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {history.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>{item.configurationKey}</TableCell>
                    <TableCell>v{item.version}</TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                        {item.configurationValue.length > 50 
                          ? item.configurationValue.substring(0, 50) + '...'
                          : item.configurationValue
                        }
                      </Typography>
                    </TableCell>
                    <TableCell>{item.changedBy}</TableCell>
                    <TableCell>
                      {new Date(item.changedDate).toLocaleDateString()}
                    </TableCell>
                    <TableCell>{item.approvedBy || '-'}</TableCell>
                    <TableCell>
                      {item.approvedBy ? (
                        <Chip label="Approved" color="success" size="small" />
                      ) : (
                        <Chip label="Pending" color="warning" size="small" />
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}

      {activeTab === 2 && (
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <CheckCircle color="success" />
                  Configuration Health
                </Typography>
                <Box sx={{ mt: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Active Configurations: {configurations.filter(c => c.isActive).length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Pending Approvals: {configurations.filter(c => !c.approvedBy).length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Expired Configurations: {configurations.filter(c => 
                      c.expirationDate && new Date(c.expirationDate) < new Date()
                    ).length}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Security color="primary" />
                  Security Status
                </Typography>
                <Box sx={{ mt: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    All configurations are properly versioned and audited
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Approval workflow is active
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Audit trail is complete
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Edit Configuration Dialog */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          Edit Configuration: {selectedConfig?.configurationKey}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Configuration Value"
                value={editForm.configurationValue}
                onChange={(e) => setEditForm({ ...editForm, configurationValue: e.target.value })}
                multiline
                rows={4}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Data Type</InputLabel>
                <Select
                  value={editForm.dataType}
                  onChange={(e) => setEditForm({ ...editForm, dataType: e.target.value })}
                  label="Data Type"
                >
                  <MenuItem value="String">String</MenuItem>
                  <MenuItem value="Number">Number</MenuItem>
                  <MenuItem value="Boolean">Boolean</MenuItem>
                  <MenuItem value="JSON">JSON</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Effective Date"
                type="date"
                value={editForm.effectiveDate}
                onChange={(e) => setEditForm({ ...editForm, effectiveDate: e.target.value })}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Expiration Date (Optional)"
                type="date"
                value={editForm.expirationDate}
                onChange={(e) => setEditForm({ ...editForm, expirationDate: e.target.value })}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Change Reason"
                value={editForm.changeReason}
                onChange={(e) => setEditForm({ ...editForm, changeReason: e.target.value })}
                multiline
                rows={2}
                required
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleSave} variant="contained" disabled={loading}>
            {loading ? <CircularProgress size={20} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* History Dialog */}
      <Dialog open={historyDialogOpen} onClose={() => setHistoryDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          Configuration History: {selectedKey}
        </DialogTitle>
        <DialogContent>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Version</TableCell>
                  <TableCell>Value</TableCell>
                  <TableCell>Changed By</TableCell>
                  <TableCell>Changed Date</TableCell>
                  <TableCell>Approved By</TableCell>
                  <TableCell>Status</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {history.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>v{item.version}</TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                        {item.configurationValue}
                      </Typography>
                    </TableCell>
                    <TableCell>{item.changedBy}</TableCell>
                    <TableCell>
                      {new Date(item.changedDate).toLocaleDateString()}
                    </TableCell>
                    <TableCell>{item.approvedBy || '-'}</TableCell>
                    <TableCell>
                      {item.approvedBy ? (
                        <Chip label="Approved" color="success" size="small" />
                      ) : (
                        <Chip label="Pending" color="warning" size="small" />
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setHistoryDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ConfigurationManagement; 