// CoralLedger Blue - Mobile JavaScript Utilities

// ==========================================================================
// Online/Offline Status Handler
// ==========================================================================
let onlineStatusHandler = null;

window.registerOnlineStatusHandler = function (dotNetRef) {
    onlineStatusHandler = dotNetRef;

    // Initial status
    dotNetRef.invokeMethodAsync('UpdateOnlineStatus', navigator.onLine);

    // Listen for changes
    window.addEventListener('online', () => {
        dotNetRef.invokeMethodAsync('UpdateOnlineStatus', true);
    });

    window.addEventListener('offline', () => {
        dotNetRef.invokeMethodAsync('UpdateOnlineStatus', false);
    });
};

window.unregisterOnlineStatusHandler = function () {
    onlineStatusHandler = null;
};

// ==========================================================================
// Touch Gesture Detection
// ==========================================================================
class TouchGestureHandler {
    constructor(element, options = {}) {
        this.element = element;
        this.options = {
            swipeThreshold: 50,
            swipeVelocityThreshold: 0.3,
            tapTimeout: 200,
            longPressTimeout: 500,
            ...options
        };

        this.touchStart = { x: 0, y: 0, time: 0 };
        this.touchEnd = { x: 0, y: 0, time: 0 };
        this.longPressTimer = null;

        this.bindEvents();
    }

    bindEvents() {
        this.element.addEventListener('touchstart', this.onTouchStart.bind(this), { passive: true });
        this.element.addEventListener('touchmove', this.onTouchMove.bind(this), { passive: true });
        this.element.addEventListener('touchend', this.onTouchEnd.bind(this), { passive: true });
    }

    onTouchStart(e) {
        const touch = e.touches[0];
        this.touchStart = {
            x: touch.clientX,
            y: touch.clientY,
            time: Date.now()
        };

        // Long press detection
        this.longPressTimer = setTimeout(() => {
            this.element.dispatchEvent(new CustomEvent('longpress', {
                detail: { x: touch.clientX, y: touch.clientY }
            }));
        }, this.options.longPressTimeout);
    }

    onTouchMove(e) {
        // Cancel long press if moved
        if (this.longPressTimer) {
            clearTimeout(this.longPressTimer);
            this.longPressTimer = null;
        }
    }

    onTouchEnd(e) {
        if (this.longPressTimer) {
            clearTimeout(this.longPressTimer);
            this.longPressTimer = null;
        }

        const touch = e.changedTouches[0];
        this.touchEnd = {
            x: touch.clientX,
            y: touch.clientY,
            time: Date.now()
        };

        this.detectGesture();
    }

    detectGesture() {
        const deltaX = this.touchEnd.x - this.touchStart.x;
        const deltaY = this.touchEnd.y - this.touchStart.y;
        const deltaTime = this.touchEnd.time - this.touchStart.time;
        const velocity = Math.sqrt(deltaX * deltaX + deltaY * deltaY) / deltaTime;

        // Tap detection
        if (Math.abs(deltaX) < 10 && Math.abs(deltaY) < 10 && deltaTime < this.options.tapTimeout) {
            this.element.dispatchEvent(new CustomEvent('tap', {
                detail: { x: this.touchEnd.x, y: this.touchEnd.y }
            }));
            return;
        }

        // Swipe detection
        if (velocity > this.options.swipeVelocityThreshold) {
            const absX = Math.abs(deltaX);
            const absY = Math.abs(deltaY);

            if (absX > this.options.swipeThreshold || absY > this.options.swipeThreshold) {
                let direction;
                if (absX > absY) {
                    direction = deltaX > 0 ? 'right' : 'left';
                } else {
                    direction = deltaY > 0 ? 'down' : 'up';
                }

                this.element.dispatchEvent(new CustomEvent('swipe', {
                    detail: { direction, velocity, deltaX, deltaY }
                }));
            }
        }
    }
}

window.initTouchGestures = function (elementId, options) {
    const element = document.getElementById(elementId);
    if (element) {
        return new TouchGestureHandler(element, options);
    }
    return null;
};

// ==========================================================================
// Pull to Refresh
// ==========================================================================
class PullToRefresh {
    constructor(element, onRefresh) {
        this.element = element;
        this.onRefresh = onRefresh;
        this.pullDistance = 0;
        this.threshold = 80;
        this.isPulling = false;
        this.isRefreshing = false;

        this.indicator = this.createIndicator();
        this.bindEvents();
    }

    createIndicator() {
        const indicator = document.createElement('div');
        indicator.className = 'pull-to-refresh-indicator';
        indicator.innerHTML = `
            <svg class="spinner-marine" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 12a9 9 0 1 1-6.219-8.56"/>
            </svg>
        `;
        indicator.style.display = 'none';
        this.element.insertBefore(indicator, this.element.firstChild);
        return indicator;
    }

    bindEvents() {
        this.element.addEventListener('touchstart', this.onTouchStart.bind(this), { passive: true });
        this.element.addEventListener('touchmove', this.onTouchMove.bind(this), { passive: false });
        this.element.addEventListener('touchend', this.onTouchEnd.bind(this), { passive: true });
    }

    onTouchStart(e) {
        if (this.element.scrollTop === 0 && !this.isRefreshing) {
            this.startY = e.touches[0].clientY;
            this.isPulling = true;
        }
    }

    onTouchMove(e) {
        if (!this.isPulling || this.isRefreshing) return;

        const currentY = e.touches[0].clientY;
        this.pullDistance = Math.max(0, currentY - this.startY);

        if (this.pullDistance > 0) {
            e.preventDefault();
            this.indicator.style.display = 'flex';
            this.indicator.style.transform = `translateX(-50%) translateY(${Math.min(this.pullDistance, this.threshold)}px)`;

            if (this.pullDistance > this.threshold) {
                this.element.classList.add('pulling');
            }
        }
    }

    async onTouchEnd() {
        if (!this.isPulling) return;

        this.isPulling = false;
        this.element.classList.remove('pulling');

        if (this.pullDistance > this.threshold) {
            this.isRefreshing = true;
            this.element.classList.add('refreshing');

            try {
                await this.onRefresh();
            } finally {
                this.isRefreshing = false;
                this.element.classList.remove('refreshing');
            }
        }

        this.indicator.style.display = 'none';
        this.indicator.style.transform = '';
        this.pullDistance = 0;
    }
}

window.initPullToRefresh = function (elementId, dotNetRef, methodName) {
    const element = document.getElementById(elementId);
    if (element) {
        return new PullToRefresh(element, async () => {
            await dotNetRef.invokeMethodAsync(methodName);
        });
    }
    return null;
};

// ==========================================================================
// Haptic Feedback (if available)
// ==========================================================================
window.triggerHaptic = function (type = 'light') {
    if ('vibrate' in navigator) {
        switch (type) {
            case 'light':
                navigator.vibrate(10);
                break;
            case 'medium':
                navigator.vibrate(20);
                break;
            case 'heavy':
                navigator.vibrate(30);
                break;
            case 'success':
                navigator.vibrate([10, 50, 10]);
                break;
            case 'error':
                navigator.vibrate([30, 50, 30, 50, 30]);
                break;
        }
    }
};

// ==========================================================================
// Scroll Lock for Modals
// ==========================================================================
let scrollPosition = 0;

window.lockBodyScroll = function () {
    scrollPosition = window.pageYOffset;
    document.body.style.overflow = 'hidden';
    document.body.style.position = 'fixed';
    document.body.style.top = `-${scrollPosition}px`;
    document.body.style.width = '100%';
};

window.unlockBodyScroll = function () {
    document.body.style.removeProperty('overflow');
    document.body.style.removeProperty('position');
    document.body.style.removeProperty('top');
    document.body.style.removeProperty('width');
    window.scrollTo(0, scrollPosition);
};

// ==========================================================================
// Viewport Height Fix for Mobile (iOS Safari)
// ==========================================================================
function setVH() {
    const vh = window.innerHeight * 0.01;
    document.documentElement.style.setProperty('--vh', `${vh}px`);
}

window.addEventListener('resize', setVH);
window.addEventListener('orientationchange', () => {
    setTimeout(setVH, 100);
});
setVH();

// ==========================================================================
// Prevent Overscroll/Bounce on iOS
// ==========================================================================
document.addEventListener('touchmove', function (e) {
    const target = e.target;
    // Allow scrolling within scrollable elements
    if (target.closest('.scrollable, .bottom-sheet-content, .pull-to-refresh')) {
        return;
    }
    // Prevent bounce on body
    if (document.body.scrollHeight <= window.innerHeight) {
        e.preventDefault();
    }
}, { passive: false });

// ==========================================================================
// Install PWA Prompt
// ==========================================================================
let deferredPrompt = null;

window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    deferredPrompt = e;

    // Dispatch custom event for Blazor to handle
    window.dispatchEvent(new CustomEvent('pwainstallable'));
});

window.showInstallPrompt = async function () {
    if (deferredPrompt) {
        deferredPrompt.prompt();
        const { outcome } = await deferredPrompt.userChoice;
        deferredPrompt = null;
        return outcome === 'accepted';
    }
    return false;
};

window.isPWAInstalled = function () {
    return window.matchMedia('(display-mode: standalone)').matches ||
           window.navigator.standalone === true;
};

console.log('CoralLedger Blue mobile.js loaded');
