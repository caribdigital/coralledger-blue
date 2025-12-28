/**
 * Push Notifications JavaScript Interop
 * Handles Web Push API integration for CoralLedger Blue
 */

window.pushNotifications = {
    /**
     * Check if push notifications are supported
     */
    isSupported: function() {
        return 'serviceWorker' in navigator &&
               'PushManager' in window &&
               'Notification' in window;
    },

    /**
     * Get the current permission state
     * @returns {string} 'granted', 'denied', or 'default'
     */
    getPermissionState: function() {
        if (!this.isSupported()) {
            return 'denied';
        }
        return Notification.permission;
    },

    /**
     * Check if user is currently subscribed
     * @returns {Promise<boolean>}
     */
    isSubscribed: async function() {
        if (!this.isSupported()) {
            return false;
        }

        try {
            const registration = await navigator.serviceWorker.ready;
            const subscription = await registration.pushManager.getSubscription();
            return subscription !== null;
        } catch (error) {
            console.error('Error checking subscription:', error);
            return false;
        }
    },

    /**
     * Subscribe to push notifications
     * @returns {Promise<boolean>} Success status
     */
    subscribe: async function() {
        if (!this.isSupported()) {
            throw new Error('Push notifications not supported');
        }

        try {
            // Request permission
            const permission = await Notification.requestPermission();
            if (permission !== 'granted') {
                console.log('Notification permission denied');
                return false;
            }

            // Get service worker registration
            const registration = await navigator.serviceWorker.ready;

            // Check for existing subscription
            let subscription = await registration.pushManager.getSubscription();

            if (!subscription) {
                // Get VAPID public key from server
                const response = await fetch('/api/push/vapid-public-key');
                if (!response.ok) {
                    // If API doesn't exist yet, use a placeholder approach
                    console.log('VAPID key endpoint not available, using local storage only');
                    localStorage.setItem('push-notifications-enabled', 'true');
                    return true;
                }

                const vapidPublicKey = await response.text();

                // Subscribe to push
                subscription = await registration.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: this.urlBase64ToUint8Array(vapidPublicKey)
                });

                // Send subscription to server
                await fetch('/api/push/subscribe', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(subscription)
                });
            }

            localStorage.setItem('push-notifications-enabled', 'true');
            console.log('Push notification subscription successful');
            return true;
        } catch (error) {
            console.error('Error subscribing to push:', error);
            // Fall back to local storage for UI state
            if (Notification.permission === 'granted') {
                localStorage.setItem('push-notifications-enabled', 'true');
                return true;
            }
            throw error;
        }
    },

    /**
     * Unsubscribe from push notifications
     * @returns {Promise<void>}
     */
    unsubscribe: async function() {
        try {
            if (this.isSupported()) {
                const registration = await navigator.serviceWorker.ready;
                const subscription = await registration.pushManager.getSubscription();

                if (subscription) {
                    // Notify server
                    try {
                        await fetch('/api/push/unsubscribe', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify(subscription)
                        });
                    } catch {
                        // Server call optional
                    }

                    await subscription.unsubscribe();
                }
            }

            localStorage.removeItem('push-notifications-enabled');
            console.log('Push notification unsubscribed');
        } catch (error) {
            console.error('Error unsubscribing:', error);
            localStorage.removeItem('push-notifications-enabled');
        }
    },

    /**
     * Convert VAPID public key from base64 to Uint8Array
     */
    urlBase64ToUint8Array: function(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/\-/g, '+')
            .replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    },

    /**
     * Show a local notification (for testing)
     */
    showTestNotification: async function(title, body) {
        if (!this.isSupported()) {
            return;
        }

        if (Notification.permission !== 'granted') {
            return;
        }

        const registration = await navigator.serviceWorker.ready;
        await registration.showNotification(title, {
            body: body,
            icon: '/icon-192.png',
            badge: '/icon-72.png',
            tag: 'test-notification',
            requireInteraction: false,
            data: {
                url: '/alerts'
            }
        });
    }
};
