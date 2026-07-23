# Kurdost AI - Development TODO

## Phase 1: Chat Tab Improvements (High Priority)

### 1.1 Copy Message Feature
- [ ] Add copy button to each chat message
- [ ] Implement copy to clipboard functionality
- [ ] Add visual feedback when message is copied
- [ ] Test copy functionality for both user and AI messages

### 1.2 Delete Individual Messages
- [ ] Add delete button to each chat message
- [ ] Implement delete message functionality
- [ ] Add confirmation dialog before deletion
- [ ] Update chat history after deletion
- [ ] Test delete functionality

### 1.3 Export Chat History
- [ ] Add export button in Chat tab
- [ ] Implement export to JSON format
- [ ] Implement export to TXT format
- [ ] Add file save dialog
- [ ] Test export functionality

### 1.4 Auto-scroll to New Messages
- [ ] Implement auto-scroll when new message arrives
- [ ] Add toggle for auto-scroll in settings
- [ ] Scroll to bottom when user sends message
- [ ] Scroll to bottom when AI responds
- [ ] Test auto-scroll behavior

### 1.5 Timestamps for Messages
- [ ] Add timestamp field to ChatMessage class
- [ ] Store timestamp when message is created
- [ ] Display timestamp in chat UI
- [ ] Format timestamp in readable format
- [ ] Test timestamp display

### 1.6 Loading Indicator
- [ ] Add animated loading indicator
- [ ] Show loading when request is in progress
- [ ] Hide loading when response arrives
- [ ] Add loading spinner or progress bar
- [ ] Test loading indicator

### 1.7 Retry Failed Messages
- [ ] Add retry button to failed messages
- [ ] Implement retry functionality
- [ ] Resend original message on retry
- [ ] Update message status after retry
- [ ] Test retry functionality

### 1.8 Markdown Support
- [ ] Add Markdown parsing library
- [ ] Implement Markdown rendering for AI responses
- [ ] Support basic formatting (bold, italic, code blocks)
- [ ] Support lists and headers
- [ ] Test Markdown rendering

---

## Phase 2: Settings Tab Improvements (High Priority)

### 2.1 Provider Selection
- [ ] Add provider dropdown (Groq/Gemini)
- [ ] Save provider preference to EditorPrefs
- [ ] Load provider preference on startup
- [ ] Update backend request with selected provider
- [ ] Test provider switching

### 2.2 Model Selection
- [ ] Add model dropdown based on provider
- [ ] Fetch available models from backend
- [ ] Save model preference to EditorPrefs
- [ ] Load model preference on startup
- [ ] Test model selection

### 2.3 Temperature Control
- [ ] Add temperature slider (0.0 - 2.0)
- [ ] Display current temperature value
- [ ] Save temperature to EditorPrefs
- [ ] Load temperature on startup
- [ ] Send temperature in backend request
- [ ] Test temperature control

### 2.4 Max Tokens Control
- [ ] Add max tokens input field
- [ ] Set reasonable default (1000)
- [ ] Save max tokens to EditorPrefs
- [ ] Load max tokens on startup
- [ ] Send max tokens in backend request
- [ ] Test max tokens control

### 2.5 Custom Server URL
- [ ] Add server URL input field
- [ ] Save custom URL to EditorPrefs
- [ ] Load custom URL on startup
- [ ] Validate URL format
- [ ] Use custom URL in requests
- [ ] Test custom URL functionality

### 2.6 Dynamic Server Status
- [ ] Implement server health check
- [ ] Add ping functionality
- [ ] Update status indicator based on response
- [ ] Show green for connected, red for disconnected
- [ ] Add refresh button for status check
- [ ] Test dynamic status

### 2.7 Language Preference
- [ ] Add language dropdown (Arabic/English)
- [ ] Save language preference to EditorPrefs
- [ ] Load language preference on startup
- [ ] Update UI language based on preference
- [ ] Test language switching

### 2.8 Theme Selection
- [ ] Add theme dropdown (Dark/Light)
- [ ] Implement dark theme colors
- [ ] Implement light theme colors
- [ ] Save theme preference to EditorPrefs
- [ ] Load theme on startup
- [ ] Apply theme to all UI elements
- [ ] Test theme switching

---

## Phase 3: Tools Tab Implementation (Medium Priority)

### 3.1 Script Analysis Tool
- [ ] Add script selection interface
- [ ] Implement script reading functionality
- [ ] Send script content to AI for analysis
- [ ] Display analysis results
- [ ] Add copy analysis button
- [ ] Test script analysis

### 3.2 Error Fixing Tool
- [ ] Add console error reading
- [ ] Parse error messages
- [ ] Send errors to AI for solutions
- [ ] Display suggested fixes
- [ ] Add apply fix button
- [ ] Test error fixing

### 3.3 Code Generation Tool
- [ ] Add code prompt input
- [ ] Implement code generation request
- [ ] Display generated code
- [ ] Add copy code button
- [ ] Add create script button
- [ ] Test code generation

---

## Phase 4: General UI Improvements (Low Priority)

### 4.1 Keyboard Shortcuts
- [ ] Add Ctrl+Enter to send message
- [ ] Add Ctrl+C to copy selected message
- [ ] Add Ctrl+D to delete selected message
- [ ] Add Ctrl+E to export chat
- [ ] Add Ctrl+N for new chat
- [ ] Display shortcuts in help menu
- [ ] Test all shortcuts

### 4.2 Tooltips
- [ ] Add tooltips to all buttons
- [ ] Add tooltips to input fields
- [ ] Add tooltips to settings options
- [ ] Implement tooltip display on hover
- [ ] Test tooltip functionality

### 4.3 Notifications System
- [ ] Implement notification system
- [ ] Show success notifications
- [ ] Show error notifications
- [ ] Show warning notifications
- [ ] Add notification history
- [ ] Test notifications

### 4.4 Confirmation Dialogs
- [ ] Add confirmation for clearing chat
- [ ] Add confirmation for logout
- [ ] Add confirmation for export
- [ ] Add confirmation for settings reset
- [ ] Test all confirmations

### 4.5 Responsive Design
- [ ] Test UI at minimum size (400x500)
- [ ] Test UI at larger sizes
- [ ] Adjust layout for different sizes
- [ ] Ensure all elements are visible
- [ ] Test responsive behavior

---

## Phase 5: Backend Improvements

### 5.1 Enhanced Error Handling
- [x] Add detailed error logging
- [ ] Implement error recovery
- [ ] Add rate limiting handling
- [x] Add timeout handling
- [ ] Test error scenarios

### 5.2 Request Queue
- [ ] Implement request queue
- [ ] Handle concurrent requests
- [ ] Add request priority
- [ ] Show queue status
- [ ] Test queue functionality

### 5.3 Response Caching
- [ ] Implement response caching
- [ ] Add cache invalidation
- [ ] Show cache status
- [ ] Test caching behavior

### 5.4 Dynamic Configuration Support
- [x] Accept model from request
- [x] Accept temperature from request
- [x] Accept max_tokens from request
- [x] Update GroqProvider to use dynamic config

---

## Phase 6: Testing & Documentation

### 6.1 Unit Tests
- [ ] Write unit tests for chat functionality
- [ ] Write unit tests for settings
- [ ] Write unit tests for tools
- [ ] Run all tests
- [ ] Fix any failing tests

### 6.2 Integration Tests
- [ ] Test backend integration
- [ ] Test API key handling
- [ ] Test error handling
- [ ] Test all user flows

### 6.3 Documentation
- [ ] Update README with new features
- [ ] Add user guide
- [ ] Add developer guide
- [ ] Add API documentation
- [ ] Add troubleshooting guide

---

## Phase 7: Deployment

### 7.1 Backend Deployment
- [ ] Update backend on Render
- [ ] Test deployed backend
- [ ] Monitor backend logs
- [ ] Fix any deployment issues

### 7.2 Frontend Deployment
- [ ] Update Unity package
- [ ] Test package installation
- [ ] Update package version
- [ ] Release new version

---

## Notes

- Prioritize Phase 1 and Phase 2 as they are high priority
- Test each feature thoroughly before moving to the next
- Keep user experience in mind when implementing features
- Maintain code quality and follow best practices
- Document any breaking changes
