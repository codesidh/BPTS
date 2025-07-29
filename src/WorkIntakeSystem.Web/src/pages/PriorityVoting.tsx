import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Slider,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  Tab,
  Alert,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Rating,
  Divider
} from '@mui/material';
import {
  ThumbUp,
  ThumbDown,
  TrendingUp,
  Schedule,
  CheckCircle,
  Warning,
  Info
} from '@mui/icons-material';
import { apiService } from '../services/api';
import { PriorityVote, WorkCategory, PriorityLevel } from '../types';

interface PendingVote {
  workRequestId: number;
  title: string;
  category: WorkCategory;
  priorityLevel: PriorityLevel;
  createdDate: string;
  submitterName: string;
  daysSinceCreation: number;
}

interface VoteDetail {
  departmentId: number;
  departmentName: string;
  vote: PriorityVote;
  businessValueScore: number;
  strategicAlignment: number;
  comments?: string;
  votedBy: string;
  votedDate: string;
}

interface PriorityVotingStatus {
  workRequestId: number;
  workRequestTitle: string;
  totalDepartments: number;
  votedDepartments: number;
  pendingDepartments: number;
  currentPriorityScore: number;
  currentPriorityLevel: PriorityLevel;
  votes: VoteDetail[];
}

interface DepartmentVotingPattern {
  departmentId: number;
  departmentName: string;
  totalVotes: number;
  voteDistribution: Record<PriorityVote, number>;
  averageBusinessValueScore: number;
  averageStrategicAlignment: number;
  recentVotes: Array<{
    workRequestId: number;
    vote: PriorityVote;
    businessValueScore: number;
    strategicAlignment: number;
    votedDate: string;
  }>;
}

const PriorityVoting: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [pendingVotes, setPendingVotes] = useState<PendingVote[]>([]);
  const [selectedWorkRequest, setSelectedWorkRequest] = useState<PendingVote | null>(null);
  const [votingDialogOpen, setVotingDialogOpen] = useState(false);
  const [votingStatus, setVotingStatus] = useState<PriorityVotingStatus | null>(null);
  const [departmentPatterns, setDepartmentPatterns] = useState<DepartmentVotingPattern[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Voting form state
  const [vote, setVote] = useState<PriorityVote>(PriorityVote.Medium);
  const [businessValueScore, setBusinessValueScore] = useState<number>(0.5);
  const [strategicAlignment, setStrategicAlignment] = useState<number>(0.5);
  const [comments, setComments] = useState<string>('');

  useEffect(() => {
    loadPendingVotes();
    loadDepartmentPatterns();
  }, []);

  const loadPendingVotes = async () => {
    try {
      setLoading(true);
      // Mock department ID - in real app, get from user context
      const response = await apiService.getApi().get(`/api/priority/pending/1`);
      setPendingVotes(response.data);
    } catch (err) {
      setError('Failed to load pending votes');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadDepartmentPatterns = async () => {
    try {
      // Load patterns for all departments
      const departments = [1, 2, 3, 4, 5]; // Mock department IDs
      const patterns = await Promise.all(
        departments.map(async (deptId) => {
          try {
            const response = await apiService.getApi().get(`/api/priority/patterns/${deptId}`);
            return response.data;
          } catch {
            return null;
          }
        })
      );
      setDepartmentPatterns(patterns.filter(Boolean));
    } catch (err) {
      console.error('Failed to load department patterns:', err);
    }
  };

  const handleVoteClick = async (workRequest: PendingVote) => {
    setSelectedWorkRequest(workRequest);
    setVotingDialogOpen(true);
    
    // Load current voting status
    try {
              const response = await apiService.getApi().get(`/api/priority/status/${workRequest.workRequestId}`);
      setVotingStatus(response.data);
    } catch (err) {
      console.error('Failed to load voting status:', err);
    }
  };

  const handleSubmitVote = async () => {
    if (!selectedWorkRequest) return;

    try {
      setLoading(true);
      const voteData = {
        workRequestId: selectedWorkRequest.workRequestId,
        vote,
        businessValueScore,
        strategicAlignment,
        comments
      };

              await apiService.getApi().post('/api/priority/vote', voteData);
      
      setVotingDialogOpen(false);
      setSelectedWorkRequest(null);
      resetVoteForm();
      loadPendingVotes();
      
      // Show success message
      setError(null);
    } catch (err) {
      setError('Failed to submit vote');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const resetVoteForm = () => {
    setVote(PriorityVote.Medium);
    setBusinessValueScore(0.5);
    setStrategicAlignment(0.5);
    setComments('');
  };

  const getVoteColor = (vote: PriorityVote) => {
    switch (vote) {
      case PriorityVote.High: return 'success';
      case PriorityVote.Medium: return 'warning';
      case PriorityVote.Low: return 'error';
      default: return 'default';
    }
  };

  const getVoteIcon = (vote: PriorityVote) => {
    switch (vote) {
      case PriorityVote.High: return <ThumbUp />;
      case PriorityVote.Medium: return <TrendingUp />;
      case PriorityVote.Low: return <ThumbDown />;
      default: return <Info />;
    }
  };

  const getPriorityColor = (level: PriorityLevel) => {
    switch (level) {
      case PriorityLevel.Critical: return 'error';
      case PriorityLevel.High: return 'warning';
      case PriorityLevel.Medium: return 'info';
      case PriorityLevel.Low: return 'success';
      default: return 'default';
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Priority Voting System
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Paper sx={{ mb: 3 }}>
        <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)}>
          <Tab label="Pending Votes" icon={<Schedule />} />
          <Tab label="Voting Patterns" icon={<TrendingUp />} />
          <Tab label="Voting Status" icon={<CheckCircle />} />
        </Tabs>
      </Paper>

      {activeTab === 0 && (
        <Box>
          <Typography variant="h6" gutterBottom>
            Pending Votes ({pendingVotes.length})
          </Typography>
          
          {loading ? (
            <Box display="flex" justifyContent="center" p={3}>
              <CircularProgress />
            </Box>
          ) : (
            <Grid container spacing={2}>
              {pendingVotes.map((vote) => (
                <Grid item xs={12} md={6} lg={4} key={vote.workRequestId}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        {vote.title}
                      </Typography>
                      
                      <Box sx={{ mb: 2 }}>
                        <Chip 
                          label={vote.category} 
                          size="small" 
                          sx={{ mr: 1 }} 
                        />
                        <Chip 
                          label={vote.priorityLevel} 
                          color={getPriorityColor(vote.priorityLevel) as any}
                          size="small" 
                        />
                      </Box>
                      
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        Submitted by: {vote.submitterName}
                      </Typography>
                      
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        Created: {new Date(vote.createdDate).toLocaleDateString()} 
                        ({vote.daysSinceCreation} days ago)
                      </Typography>
                      
                      <Button
                        variant="contained"
                        color="primary"
                        fullWidth
                        onClick={() => handleVoteClick(vote)}
                        sx={{ mt: 2 }}
                      >
                        Vote Now
                      </Button>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
          )}
        </Box>
      )}

      {activeTab === 1 && (
        <Box>
          <Typography variant="h6" gutterBottom>
            Department Voting Patterns
          </Typography>
          
          <Grid container spacing={3}>
            {departmentPatterns.map((pattern) => (
              <Grid item xs={12} md={6} key={pattern.departmentId}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      {pattern.departmentName}
                    </Typography>
                    
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Total Votes: {pattern.totalVotes}
                    </Typography>
                    
                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" gutterBottom>
                        Vote Distribution:
                      </Typography>
                      <Box display="flex" gap={1} flexWrap="wrap">
                        {Object.entries(pattern.voteDistribution).map(([voteType, count]) => (
                          <Chip
                            key={voteType}
                            label={`${voteType}: ${count}`}
                            color={getVoteColor(voteType as PriorityVote) as any}
                            size="small"
                            icon={getVoteIcon(voteType as PriorityVote)}
                          />
                        ))}
                      </Box>
                    </Box>
                    
                    <Divider sx={{ my: 2 }} />
                    
                    <Typography variant="body2" gutterBottom>
                      Average Business Value Score: {(pattern.averageBusinessValueScore * 100).toFixed(1)}%
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      Average Strategic Alignment: {(pattern.averageStrategicAlignment * 100).toFixed(1)}%
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Box>
      )}

      {activeTab === 2 && votingStatus && (
        <Box>
          <Typography variant="h6" gutterBottom>
            Voting Status: {votingStatus.workRequestTitle}
          </Typography>
          
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Summary
                  </Typography>
                  
                  <Typography variant="body2" gutterBottom>
                    Total Departments: {votingStatus.totalDepartments}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    Voted: {votingStatus.votedDepartments}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    Pending: {votingStatus.pendingDepartments}
                  </Typography>
                  
                  <Divider sx={{ my: 2 }} />
                  
                  <Typography variant="body2" gutterBottom>
                    Current Priority Score: {(votingStatus.currentPriorityScore * 100).toFixed(1)}%
                  </Typography>
                  <Chip 
                    label={votingStatus.currentPriorityLevel} 
                    color={getPriorityColor(votingStatus.currentPriorityLevel) as any}
                  />
                </CardContent>
              </Card>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Recent Votes
                  </Typography>
                  
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Department</TableCell>
                          <TableCell>Vote</TableCell>
                          <TableCell>Business Value</TableCell>
                          <TableCell>Strategic</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {votingStatus.votes.map((vote) => (
                          <TableRow key={vote.departmentId}>
                            <TableCell>{vote.departmentName}</TableCell>
                            <TableCell>
                              <Chip
                                label={vote.vote}
                                color={getVoteColor(vote.vote) as any}
                                size="small"
                                icon={getVoteIcon(vote.vote)}
                              />
                            </TableCell>
                            <TableCell>{(vote.businessValueScore * 100).toFixed(0)}%</TableCell>
                            <TableCell>{(vote.strategicAlignment * 100).toFixed(0)}%</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </Box>
      )}

      {/* Voting Dialog */}
      <Dialog 
        open={votingDialogOpen} 
        onClose={() => setVotingDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          Vote on Work Request: {selectedWorkRequest?.title}
        </DialogTitle>
        
        <DialogContent>
          <Grid container spacing={3} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Priority Vote</InputLabel>
                <Select
                  value={vote}
                  onChange={(e) => setVote(e.target.value as PriorityVote)}
                  label="Priority Vote"
                >
                  <MenuItem value={PriorityVote.High}>High Priority</MenuItem>
                  <MenuItem value={PriorityVote.Medium}>Medium Priority</MenuItem>
                  <MenuItem value={PriorityVote.Low}>Low Priority</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            
            <Grid item xs={12}>
              <Typography gutterBottom>Business Value Score</Typography>
              <Slider
                value={businessValueScore}
                onChange={(_, value) => setBusinessValueScore(value as number)}
                min={0}
                max={1}
                step={0.1}
                marks
                valueLabelDisplay="auto"
                valueLabelFormat={(value) => `${(value * 100).toFixed(0)}%`}
              />
            </Grid>
            
            <Grid item xs={12}>
              <Typography gutterBottom>Strategic Alignment</Typography>
              <Slider
                value={strategicAlignment}
                onChange={(_, value) => setStrategicAlignment(value as number)}
                min={0}
                max={1}
                step={0.1}
                marks
                valueLabelDisplay="auto"
                valueLabelFormat={(value) => `${(value * 100).toFixed(0)}%`}
              />
            </Grid>
            
            <Grid item xs={12}>
              <TextField
                fullWidth
                multiline
                rows={3}
                label="Comments (Optional)"
                value={comments}
                onChange={(e) => setComments(e.target.value)}
              />
            </Grid>
          </Grid>
        </DialogContent>
        
        <DialogActions>
          <Button onClick={() => setVotingDialogOpen(false)}>
            Cancel
          </Button>
          <Button 
            onClick={handleSubmitVote} 
            variant="contained"
            disabled={loading}
          >
            {loading ? <CircularProgress size={20} /> : 'Submit Vote'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PriorityVoting;