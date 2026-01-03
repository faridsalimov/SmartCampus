document.addEventListener('DOMContentLoaded', async function() {
    'use strict';
    
    if (!window.location.pathname.includes('/Messages')) {
        return;
    }
    
    let signalRLoaded = false;
    let attemptCount = 0;
    const maxAttempts = 30;

    while (!signalRLoaded && attemptCount < maxAttempts) {
        if (typeof signalR !== 'undefined' && signalR.HubConnectionBuilder) {
            console.log('SignalR library loaded successfully');
            signalRLoaded = true;
            break;
        }
        await new Promise(resolve => setTimeout(resolve, 500));
        attemptCount++;
    }

    if (!signalRLoaded) {
        console.error('Failed to load SignalR library after waiting');
        alert('Failed to load messaging library. Please refresh the page.');
        return;
    }
    
    const messageForm = document.getElementById('messageForm');
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const messagesArea = document.getElementById('messagesArea');
    const receiverIdInput = document.getElementById('receiverId');
    const conversationsScroll = document.querySelector('.conversations-scroll');

    if (!conversationsScroll) {
        console.log('Not on Messages page or conversations scroll not found');
        return;
    }

    let receiverId = receiverIdInput?.value || '';
    console.log('Initial Receiver ID:', receiverId);
    
    let connection = null;
    try {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/messages")
            .withAutomaticReconnect([0, 0, 1000, 3000, 5000, 10000])
            .build();

        console.log('SignalR connection created');
    } catch (err) {
        console.error('Error creating SignalR connection:', err);
        alert('Error creating messaging connection. Please refresh the page.');
        return;
    }
    
    connection.on('ReceiveMessage', function(message) {
        console.log('ReceiveMessage event:', message);
        
        const senderId = message.senderId;
        
        if (receiverId && senderId === receiverId) {
            console.log('Message is from current conversation, displaying...');
            displayMessage(message, 'received');
            
            if (connection && connection.state === signalR.HubConnectionState.Connected) {
                connection.invoke('MarkAsRead', senderId)
                    .catch(err => console.error('Error marking as read:', err));
            }
        } else {
            console.log('No active conversation, updating conversation list...');
            
            const preview = message.content?.length > 50 
                ? message.content.substring(0, 50) + "..." 
                : message.content || "";
            
            updateConversationList({
                contactId: senderId,
                contactName: message.senderName || 'Unknown',
                contactPhoto: message.senderProfilePhoto,
                lastMessageDate: message.sentDate,
                preview: preview,
                isSender: false,
                unreadCount: 1
            });
        }
    });

    connection.on('MessageSent', function(message) {
        console.log('MessageSent confirmation event:', message);
        if (message && message.content) {
            displayMessage(message, 'sent');
        }
    });

    connection.on('UpdateConversation', function(data) {
        console.log('Conversation updated:', data);
        updateConversationList(data);
    });

    connection.on('UpdateConversationUnreadCount', function(data) {
        console.log('Conversation unread count updated:', data);
        updateConversationUnreadBadge(data);
    });

    connection.on('MessagesRead', function(data) {
        console.log('Messages marked as read:', data);
        markMessagesAsReadUI(data);
    });

    connection.on('UnreadCountUpdated', function(data) {
        console.log('Unread count updated from hub:', data);
        if (window.NotificationSystem) {
            window.NotificationSystem.updateBadge(data.totalUnreadCount);
        }
    });

    connection.on('NewMessageNotification', function(data) {
        console.log('New message notification received on messages page:', data);
    });

    connection.on('Error', function(error) {
        console.error('SignalR error:', error);
    });

    connection.onreconnected(function() {
        console.log('Reconnected to SignalR');
        sendBtn.disabled = false;
    });

    connection.onreconnecting(function() {
        console.log('Reconnecting to SignalR...');
        sendBtn.disabled = true;
    });
    
    async function startConnection() {
        try {
            await connection.start();
            console.log('Connected to messages hub - state:', connection.state);
            if (messageInput) {
                messageInput.focus();
            }
        } catch (err) {
            console.error('Error starting connection:', err);
            setTimeout(startConnection, 3000);
        }
    }

    await startConnection();
    
    window.messagesHub = {
        connection: connection,
        sendMessage: async function(content) {
            if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
                console.error('Not connected to hub. State:', connection?.state);
                alert('Not connected to messaging service. Please wait...');
                return;
            }
            const currentReceiverId = receiverIdInput.value;
            console.log('Invoking SendMessage with receiverId:', currentReceiverId, 'content:', content);
            return await connection.invoke('SendMessage', currentReceiverId, content);
        }
    };
    
    if (messageForm) {
        messageForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            const content = messageInput?.value?.trim();
            const currentReceiverId = receiverIdInput?.value;

            console.log('Form submitted. Content:', content, 'ReceiverID:', currentReceiverId, 'Connection state:', connection?.state);

            if (!content || !currentReceiverId) {
                console.log('Empty message or no receiver');
                return;
            }

            if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
                console.error('Not connected to hub. State:', connection?.state);
                alert('Not connected to messaging service. Reconnecting...');
                return;
            }

            console.log('Sending message via SignalR:', content);
            
            messageInput.value = '';
            messageInput.style.height = 'auto';
            sendBtn.disabled = true;

            try {
                const result = await connection.invoke('SendMessage', currentReceiverId, content);
                console.log('SendMessage invoke completed, result:', result);
                sendBtn.disabled = false;
                messageInput.focus();
            } catch (err) {
                console.error('Error sending message:', err);
                messageInput.value = content;
                sendBtn.disabled = false;
                alert('Error sending message. Please try again.');
            }
        });
    }
    
    function displayMessage(message, type) {
        console.log('displayMessage called with:', message, 'type:', type);
        
        if (!messagesArea) {
            console.error('messagesArea not found!');
            return;
        }

        const messageBubble = document.createElement('div');
        messageBubble.className = `message-bubble ${type}`;
        messageBubble.setAttribute('data-message-id', message.id || message.Id || '');
        
        if (type === 'received' && message.senderProfilePhoto) {
            const avatar = document.createElement('img');
            avatar.src = message.senderProfilePhoto;
            avatar.alt = message.senderName || 'User';
            avatar.className = 'message-bubble-avatar';
            messageBubble.appendChild(avatar);
        }

        const bubbleContent = document.createElement('div');
        bubbleContent.className = 'message-bubble-content';

        const messageText = document.createElement('div');
        messageText.className = 'message-text';
        messageText.textContent = message.content || message.Content || '';

        const messageFooter = document.createElement('div');
        messageFooter.className = 'message-footer';

        const messageTime = document.createElement('div');
        messageTime.className = 'message-time';
        
        let sentDate = null;
        const dateValue = message.sentDate || message.SentDate;
        
        if (dateValue) {
            sentDate = new Date(dateValue);
            
            if (isNaN(sentDate.getTime())) {
                sentDate = new Date();
            }
        } else {
            sentDate = new Date();
        }
        
        messageTime.textContent = sentDate.toLocaleTimeString([], { 
            hour: '2-digit', 
            minute: '2-digit', 
            hour12: false 
        });

        const messageStatus = document.createElement('span');
        messageStatus.className = 'message-status';
        messageStatus.innerHTML = type === 'sent' 
            ? (message.isRead ? '<i class="fas fa-check-double"></i>' : '<i class="fas fa-check"></i>') 
            : '';

        messageFooter.appendChild(messageTime);
        messageFooter.appendChild(messageStatus);
        bubbleContent.appendChild(messageText);
        bubbleContent.appendChild(messageFooter);
        messageBubble.appendChild(bubbleContent);

        messagesArea.appendChild(messageBubble);
        
        setTimeout(() => {
            if (messagesArea) {
                messagesArea.scrollTop = messagesArea.scrollHeight;
            }
        }, 0);

        console.log('Message displayed successfully');
    }

    function updateConversationList(data) {
        if (!conversationsScroll) return;

        const contactId = data.contactId;
        let conversationItem = document.querySelector(`[data-contact-id="${contactId}"]`);

        if (!conversationItem) {
            const emptyState = conversationsScroll.querySelector('.empty-conversations');
            if (emptyState) {
                emptyState.style.display = 'none';
                console.log('Empty state hidden');
            }

            conversationItem = document.createElement('a');
            conversationItem.href = `?contactId=${contactId}`;
            conversationItem.className = 'conversation-item';
            conversationItem.setAttribute('data-contact-id', contactId);

            const avatar = document.createElement('img');
            avatar.src = data.contactPhoto || `https://ui-avatars.com/api/?name=${encodeURIComponent(data.contactName)}&background=random&color=fff`;
            avatar.alt = data.contactName;
            avatar.className = 'conversation-avatar';

            const content = document.createElement('div');
            content.className = 'conversation-content';

            const headerRow = document.createElement('div');
            headerRow.className = 'conversation-header-row';

            const name = document.createElement('span');
            name.className = 'conversation-name';
            name.textContent = data.contactName;

            const time = document.createElement('span');
            time.className = 'conversation-time';
            time.textContent = getTimeDisplay(new Date(data.lastMessageDate));

            headerRow.appendChild(name);
            headerRow.appendChild(time);

            if (!data.isSender && data.unreadCount > 0) {
                const badge = document.createElement('span');
                badge.className = 'conversation-unread-badge';
                badge.setAttribute('data-unread-count', data.unreadCount);
                badge.textContent = data.unreadCount > 99 ? '99+' : data.unreadCount;
                headerRow.appendChild(badge);
            }

            const preview = document.createElement('div');
            preview.className = 'conversation-preview';
            if (!data.isSender) preview.classList.add('unread');
            preview.textContent = (data.isSender ? 'You: ' : '') + data.preview;

            content.appendChild(headerRow);
            content.appendChild(preview);

            conversationItem.appendChild(avatar);
            conversationItem.appendChild(content);
            
            conversationsScroll.insertBefore(conversationItem, conversationsScroll.firstChild);
        } else {
            const preview = conversationItem.querySelector('.conversation-preview');
            if (preview) {
                preview.textContent = (data.isSender ? 'You: ' : '') + data.preview;
                if (data.isSender) {
                    preview.classList.remove('unread');
                } else {
                    preview.classList.add('unread');
                }
            }

            const time = conversationItem.querySelector('.conversation-time');
            if (time) {
                time.textContent = getTimeDisplay(new Date(data.lastMessageDate));
            }
            
            const headerRow = conversationItem.querySelector('.conversation-header-row');
            if (headerRow) {
                let badge = headerRow.querySelector('.conversation-unread-badge');
                if (!data.isSender && data.unreadCount > 0) {
                    if (!badge) {
                        badge = document.createElement('span');
                        badge.className = 'conversation-unread-badge';
                        headerRow.appendChild(badge);
                    }
                    badge.setAttribute('data-unread-count', data.unreadCount);
                    badge.textContent = data.unreadCount > 99 ? '99+' : data.unreadCount;
                } else if (badge) {
                    badge.remove();
                }
            }
            
            conversationsScroll.insertBefore(conversationItem, conversationsScroll.firstChild);
        }

        console.log('Conversation list updated');
    }

    function markMessagesAsReadUI(data) {
        if (!messagesArea) return;
        
        if (data.messageIds && Array.isArray(data.messageIds)) {
            data.messageIds.forEach(messageId => {
                const bubble = messagesArea.querySelector(`[data-message-id="${messageId}"]`);
                if (bubble) {
                    const status = bubble.querySelector('.message-status');
                    if (status) {
                        status.innerHTML = '<i class="fas fa-check-double"></i>';
                        console.log('Updated message status to read for:', messageId);
                    }
                }
            });
        } else {
            const sentMessages = messagesArea.querySelectorAll('.message-bubble.sent');
            sentMessages.forEach(bubble => {
                const status = bubble.querySelector('.message-status');
                if (status) {
                    const currentHTML = status.innerHTML;
                    if (!currentHTML.includes('fa-check-double')) {
                        status.innerHTML = '<i class="fas fa-check-double"></i>';
                        console.log('Updated message status to read');
                    }
                }
            });
        }
    }

    function updateConversationUnreadBadge(data) {
        const contactId = data.contactId;
        const conversationItem = document.querySelector(`[data-contact-id="${contactId}"]`);
        if (!conversationItem) return;

        const headerRow = conversationItem.querySelector('.conversation-header-row');
        if (!headerRow) return;

        let badge = headerRow.querySelector('.conversation-unread-badge');
        if (data.unreadCount > 0) {
            if (!badge) {
                badge = document.createElement('span');
                badge.className = 'conversation-unread-badge';
                headerRow.appendChild(badge);
            }
            badge.setAttribute('data-unread-count', data.unreadCount);
            badge.textContent = data.unreadCount > 99 ? '99+' : data.unreadCount;
        } else if (badge) {
            badge.remove();
        }

        console.log('Conversation unread badge updated:', data);
    }
    
    function updateChatHeaderTime(sentDate) {
        const chatHeaderTime = document.getElementById('chatHeaderTime');
        if (!chatHeaderTime) return;

        const now = new Date();
        let timeText = '';

        if (sentDate.toDateString() === now.toDateString()) {
            timeText = sentDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
        } else if (sentDate.toDateString() === new Date(now.getTime() - 86400000).toDateString()) {
            timeText = 'Yesterday ' + sentDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
        } else {
            timeText = sentDate.toLocaleDateString([], { month: 'short', day: 'numeric' }) + ', ' + sentDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
        }

        chatHeaderTime.textContent = timeText;
    }
    
    function getTimeDisplay(date) {
        const now = new Date();
        if (date.toDateString() === now.toDateString()) {
            return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
        } else if (date.toDateString() === new Date(now.getTime() - 86400000).toDateString()) {
            return 'Yesterday';
        } else if (date > new Date(now.getTime() - 604800000)) {
            return date.toLocaleDateString([], { weekday: 'short' });
        } else {
            return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
        }
    }
    
    if (messageInput) {
        messageInput.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = Math.min(this.scrollHeight, 100) + 'px';
        });

        messageInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                messageForm.dispatchEvent(new Event('submit'));
            }
        });
    }

    if (receiverId && connection.state === signalR.HubConnectionState.Connected) {
        console.log('Marking initial conversation as read:', receiverId);
        try {
            await connection.invoke('MarkAsRead', receiverId);
            console.log('Initial conversation marked as read');
            await connection.invoke('GetUnreadCount');
        } catch (err) {
            console.error('Error marking initial conversation as read:', err);
        }
    }

    console.log('Messages hub initialized successfully');
});