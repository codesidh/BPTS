# Phase 2 Implementation TODOs

## 1. Enhanced Priority Voting System
- [ ] Implement missing backend logic for department-weighted voting, business value input, and real-time recalculation
- [ ] Expose/extend API endpoints for voting and recalculation
- [ ] Integrate frontend UI for department voting, business value, and recalculation
- [ ] Ensure real-time update/feedback for priority changes

## 2. Advanced Workflow Engine
- [ ] Design and implement backend workflow engine/service to enforce workflow transitions
- [ ] Implement permissions and validation for stage transitions
- [ ] Trigger actions and notifications on stage changes
- [ ] Add audit/event logging for all workflow transitions
- [ ] Expose/extend API endpoints for workflow actions
- [ ] Integrate frontend UI for workflow stage management and transitions

## 3. Configuration Management
- [ ] Build backend services and APIs for managing, validating, and applying configuration changes
- [ ] Implement versioning, effective/expiration dates, and change tracking logic
- [ ] Integrate frontend UI for configuration management (view, edit, history)
- [ ] Add permissions and approval workflow for configuration changes

## 4. Event Sourcing Implementation
- [ ] Implement backend service for appending, replaying, and projecting events
- [ ] Expose API for event/audit trail viewing and replay
- [ ] Integrate frontend UI for event/audit trail viewing and replay
- [ ] Ensure all major actions (work request, workflow, config) emit events 