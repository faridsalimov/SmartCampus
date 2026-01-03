(function() {
    'use strict';

    let notificationHub = null;
    let notificationInitialized = false;

    async function initializeNotifications() {
        if (notificationInitialized) {
            return;
        }

        let signalRLoaded = false;
        let attemptCount = 0;
        const maxAttempts = 30;

        while (!signalRLoaded && attemptCount < maxAttempts) {
            if (typeof signalR !== 'undefined' && signalR.HubConnectionBuilder) {
                console.log('SignalR library available for notifications');
                signalRLoaded = true;
                break;
            }
            await new Promise(resolve => setTimeout(resolve, 500));
            attemptCount++;
        }

        if (!signalRLoaded) {
            console.warn('SignalR not available for notifications');
            return;
        }

        try {
            notificationHub = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/messages")
                .withAutomaticReconnect([0, 0, 1000, 3000, 5000, 10000])
                .build();

            notificationHub.on('NewMessageNotification', function(data) {
                console.log('New message notification received:', data);
                handleNewMessageNotification(data);
            });

            notificationHub.on('UnreadCountUpdated', function(data) {
                console.log('Unread count updated:', data);
                updateNotificationBadge(data.totalUnreadCount);
            });

            notificationHub.onreconnected(function() {
                console.log('Notification hub reconnected');
                if (notificationHub && notificationHub.state === signalR.HubConnectionState.Connected) {
                    notificationHub.invoke('GetUnreadCount').catch(err => console.error('Error getting unread count:', err));
                }
            });

            notificationHub.onreconnecting(function() {
                console.log('Notification hub reconnecting...');
            });

            await notificationHub.start();
            console.log('Notification hub connected successfully');

            if (notificationHub.state === signalR.HubConnectionState.Connected) {
                notificationHub.invoke('GetUnreadCount').catch(err => console.error('Error getting initial unread count:', err));
            }

            notificationInitialized = true;
        } catch (err) {
            console.error('Error initializing notification hub:', err);
            setTimeout(() => {
                notificationInitialized = false;
                initializeNotifications();
            }, 5000);
        }
    }

    function handleNewMessageNotification(data) {
        console.log('Processing new message notification:', data);

        updateNotificationBadge(data.totalUnreadCount);

        if (!window.location.pathname.includes('/Messages')) {
            showNotificationIndicator(data);
        }
    }

    function updateNotificationBadge(unreadCount) {
        console.log('Updating notification badge with count:', unreadCount);

        const sidebarMessagesLink = document.querySelector('[data-notification-target="messages-sidebar"]');
        if (sidebarMessagesLink) {
            let sidebarBadge = sidebarMessagesLink.querySelector('.sidebar-notification-badge');

            if (unreadCount > 0) {
                if (!sidebarBadge) {
                    sidebarBadge = document.createElement('span');
                    sidebarBadge.className = 'sidebar-notification-badge';
                    const iconDiv = sidebarMessagesLink.querySelector('.sidebar-menu-icon');
                    if (iconDiv) {
                        iconDiv.appendChild(sidebarBadge);
                    }
                    console.log('Created new sidebar notification badge');
                }
                sidebarBadge.textContent = unreadCount > 99 ? '99+' : unreadCount;
                sidebarBadge.style.display = 'flex';
                triggerBadgeAnimation(sidebarBadge);
                console.log('Sidebar badge updated with count:', unreadCount);
            } else {
                if (sidebarBadge) {
                    sidebarBadge.style.display = 'none';
                    console.log('Sidebar badge hidden (no unread messages)');
                }
            }
        }
    }

    function triggerBadgeAnimation(badgeElement) {
        if (!badgeElement) return;
        
        badgeElement.classList.remove('active-notification');
        
        void badgeElement.offsetWidth;
        
        badgeElement.classList.add('active-notification');
        
        setTimeout(() => {
            badgeElement.classList.remove('active-notification');
        }, 500);
    }

    function showNotificationIndicator(data) {
        console.log('Showing notification indicator for message from:', data.senderName);

        playNotificationSound();

        if ('Notification' in window && Notification.permission === 'granted') {
            try {
                new Notification(`New message from ${data.senderName}`, {
                    body: data.messagePreview,
                    icon: data.senderProfilePhoto || '/favicon.ico',
                    tag: 'message-notification',
                    requireInteraction: false,
                    badge: '/favicon.ico'
                });
                console.log('Browser notification shown');
            } catch (err) {
                console.error('Error showing browser notification:', err);
            }
        }
    }

    function playNotificationSound() {
        try {
            const audioContext = window.AudioContext || window.webkitAudioContext;
            if (!audioContext) {
                console.log('Web Audio API not available');
                return;
            }

            const context = new audioContext();
            const oscillator = context.createOscillator();
            const gainNode = context.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(context.destination);

            oscillator.frequency.value = 800;
            oscillator.type = 'sine';

            gainNode.gain.setValueAtTime(0.3, context.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, context.currentTime + 0.1);

            oscillator.start(context.currentTime);
            oscillator.stop(context.currentTime + 0.1);

            console.log('Notification sound played');
        } catch (err) {
            console.error('Error playing notification sound:', err);
        }
    }

    function requestNotificationPermission() {
        if ('Notification' in window && Notification.permission === 'default') {
            Notification.requestPermission();
        }
    }

    window.NotificationSystem = {
        init: initializeNotifications,
        updateBadge: updateNotificationBadge,
        requestPermission: requestNotificationPermission,
        getHub: function() {
            return notificationHub;
        }
    };

    document.addEventListener('DOMContentLoaded', function() {
        console.log('Initializing notification system on page load');
        initializeNotifications().catch(err => console.error('Error during notification initialization:', err));
        requestNotificationPermission();
    });

    document.addEventListener('visibilitychange', function() {
        if (!document.hidden && notificationHub && notificationHub.state !== signalR.HubConnectionState.Connected) {
            console.log('Page became visible, checking notification connection...');
            initializeNotifications().catch(err => console.error('Error reinitializing notifications:', err));
        }
    });
})();
