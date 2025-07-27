const CACHE_NAME = 'work-intake-system-v1.0.0';
const STATIC_CACHE_NAME = 'work-intake-static-v1.0.0';
const DYNAMIC_CACHE_NAME = 'work-intake-dynamic-v1.0.0';

// Resources to cache immediately
const STATIC_RESOURCES = [
  '/',
  '/dashboard',
  '/work-requests',
  '/priority-voting',
  '/analytics',
  '/static/js/bundle.js',
  '/static/css/main.css',
  '/manifest.json',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png'
];

// API endpoints to cache with NetworkFirst strategy
const API_CACHE_PATTERNS = [
  /^\/api\/workrequests/,
  /^\/api\/priority/,
  /^\/api\/analytics/,
  /^\/api\/users/,
  /^\/api\/departments/
];

// Install event - cache static resources
self.addEventListener('install', (event) => {
  console.log('Service Worker installing...');
  event.waitUntil(
    caches.open(STATIC_CACHE_NAME)
      .then((cache) => {
        console.log('Caching static resources');
        return cache.addAll(STATIC_RESOURCES);
      })
      .then(() => {
        return self.skipWaiting();
      })
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
  console.log('Service Worker activating...');
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.map((cacheName) => {
          if (cacheName !== STATIC_CACHE_NAME && 
              cacheName !== DYNAMIC_CACHE_NAME &&
              cacheName !== CACHE_NAME) {
            console.log('Deleting old cache:', cacheName);
            return caches.delete(cacheName);
          }
        })
      );
    }).then(() => {
      return self.clients.claim();
    })
  );
});

// Fetch event - implement caching strategies
self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  // Handle different types of requests with appropriate strategies
  if (request.method === 'GET') {
    if (isStaticResource(url.pathname)) {
      // Static resources: CacheFirst
      event.respondWith(cacheFirst(request));
    } else if (isAPIRequest(url.pathname)) {
      // API requests: NetworkFirst with fallback
      event.respondWith(networkFirst(request));
    } else if (isNavigationRequest(request)) {
      // Navigation requests: StaleWhileRevalidate
      event.respondWith(staleWhileRevalidate(request));
    } else {
      // Other requests: NetworkFirst
      event.respondWith(networkFirst(request));
    }
  }
});

// Background sync for offline actions
self.addEventListener('sync', (event) => {
  console.log('Background sync triggered:', event.tag);
  
  if (event.tag === 'offline-sync') {
    event.waitUntil(syncOfflineActions());
  }
});

// Push notification handling
self.addEventListener('push', (event) => {
  console.log('Push notification received');
  
  const options = {
    body: 'You have new updates in the Work Intake System',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/badge-72x72.png',
    vibrate: [200, 100, 200],
    data: {
      dateOfArrival: Date.now(),
      primaryKey: 1
    },
    actions: [
      {
        action: 'explore',
        title: 'View Details',
        icon: '/icons/checkmark.png'
      },
      {
        action: 'close',
        title: 'Close',
        icon: '/icons/xmark.png'
      }
    ]
  };

  if (event.data) {
    const data = event.data.json();
    options.body = data.body || options.body;
    options.data = { ...options.data, ...data };
  }

  event.waitUntil(
    self.registration.showNotification('Work Intake System', options)
  );
});

// Notification click handling
self.addEventListener('notificationclick', (event) => {
  console.log('Notification clicked:', event.notification.data);
  
  event.notification.close();

  if (event.action === 'explore') {
    event.waitUntil(
      clients.openWindow('/dashboard')
    );
  } else if (event.action === 'close') {
    // Just close the notification
  } else {
    // Default action - open the app
    event.waitUntil(
      clients.openWindow('/')
    );
  }
});

// Caching strategies
async function cacheFirst(request) {
  try {
    const cachedResponse = await caches.match(request);
    if (cachedResponse) {
      return cachedResponse;
    }

    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      const cache = await caches.open(STATIC_CACHE_NAME);
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    console.error('CacheFirst strategy failed:', error);
    return new Response('Offline', { status: 503 });
  }
}

async function networkFirst(request) {
  try {
    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      const cache = await caches.open(DYNAMIC_CACHE_NAME);
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    console.log('Network failed, trying cache:', error);
    const cachedResponse = await caches.match(request);
    if (cachedResponse) {
      return cachedResponse;
    }
    
    // Return offline fallback for API requests
    if (isAPIRequest(new URL(request.url).pathname)) {
      return new Response(
        JSON.stringify({ 
          error: 'Offline', 
          message: 'This data is not available offline' 
        }),
        { 
          status: 503,
          headers: { 'Content-Type': 'application/json' }
        }
      );
    }
    
    return new Response('Offline', { status: 503 });
  }
}

async function staleWhileRevalidate(request) {
  const cache = await caches.open(DYNAMIC_CACHE_NAME);
  const cachedResponse = await cache.match(request);

  const fetchPromise = fetch(request).then((networkResponse) => {
    if (networkResponse.ok) {
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  }).catch(() => {
    // Network failed, return cached version if available
    return cachedResponse;
  });

  return cachedResponse || fetchPromise;
}

// Helper functions
function isStaticResource(pathname) {
  return pathname.startsWith('/static/') || 
         pathname.startsWith('/icons/') ||
         pathname === '/manifest.json';
}

function isAPIRequest(pathname) {
  return pathname.startsWith('/api/') ||
         API_CACHE_PATTERNS.some(pattern => pattern.test(pathname));
}

function isNavigationRequest(request) {
  return request.mode === 'navigate' ||
         (request.method === 'GET' && request.headers.get('accept').includes('text/html'));
}

// Offline sync functionality
async function syncOfflineActions() {
  try {
    console.log('Syncing offline actions...');
    
    // Get offline actions from IndexedDB
    const offlineActions = await getOfflineActions();
    
    for (const action of offlineActions) {
      try {
        await syncAction(action);
        await removeOfflineAction(action.id);
        console.log('Synced action:', action.id);
      } catch (error) {
        console.error('Failed to sync action:', action.id, error);
      }
    }
  } catch (error) {
    console.error('Offline sync failed:', error);
  }
}

async function syncAction(action) {
  const response = await fetch('/api/mobile-accessibility/offline/sync', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(action)
  });

  if (!response.ok) {
    throw new Error(`Sync failed: ${response.status}`);
  }

  return response.json();
}

// IndexedDB operations (simplified - would use a proper IndexedDB wrapper)
async function getOfflineActions() {
  // This would interact with IndexedDB to get pending offline actions
  return [];
}

async function removeOfflineAction(actionId) {
  // This would remove the synced action from IndexedDB
  console.log('Removing offline action:', actionId);
}

// Message handling for communication with main thread
self.addEventListener('message', (event) => {
  console.log('Service Worker received message:', event.data);
  
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
  
  if (event.data && event.data.type === 'GET_CACHE_STATUS') {
    event.ports[0].postMessage({
      type: 'CACHE_STATUS',
      isOnline: navigator.onLine,
      cacheNames: [STATIC_CACHE_NAME, DYNAMIC_CACHE_NAME]
    });
  }
}); 