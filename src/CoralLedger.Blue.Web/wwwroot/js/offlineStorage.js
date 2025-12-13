/**
 * Offline Storage Module for Citizen Science Observations
 * Sprint 4.2 US-4.2.5: Enable offline draft saving and sync when connectivity returns
 *
 * Uses IndexedDB for persistent storage of observation drafts and photos
 * while offline. Automatically syncs when connectivity is restored.
 */

const DB_NAME = 'CoralLedgerOffline';
const DB_VERSION = 1;
const STORES = {
    DRAFTS: 'observationDrafts',
    PHOTOS: 'draftPhotos',
    SYNC_QUEUE: 'syncQueue'
};

let db = null;

/**
 * Initialize the IndexedDB database
 */
async function initializeDatabase() {
    return new Promise((resolve, reject) => {
        if (db) {
            resolve(db);
            return;
        }

        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onerror = () => reject(request.error);

        request.onsuccess = () => {
            db = request.result;
            console.log('[OfflineStorage] Database initialized');
            resolve(db);
        };

        request.onupgradeneeded = (event) => {
            const database = event.target.result;

            // Store for observation drafts
            if (!database.objectStoreNames.contains(STORES.DRAFTS)) {
                const draftStore = database.createObjectStore(STORES.DRAFTS, { keyPath: 'draftId' });
                draftStore.createIndex('createdAt', 'createdAt', { unique: false });
                draftStore.createIndex('status', 'status', { unique: false });
            }

            // Store for draft photos (binary data)
            if (!database.objectStoreNames.contains(STORES.PHOTOS)) {
                const photoStore = database.createObjectStore(STORES.PHOTOS, { keyPath: 'photoId' });
                photoStore.createIndex('draftId', 'draftId', { unique: false });
            }

            // Store for sync queue
            if (!database.objectStoreNames.contains(STORES.SYNC_QUEUE)) {
                const syncStore = database.createObjectStore(STORES.SYNC_QUEUE, { keyPath: 'queueId', autoIncrement: true });
                syncStore.createIndex('draftId', 'draftId', { unique: false });
                syncStore.createIndex('status', 'status', { unique: false });
            }
        };
    });
}

/**
 * Save an observation draft to IndexedDB
 * @param {Object} draft - The observation draft data
 * @returns {Promise<string>} The draft ID
 */
async function saveDraft(draft) {
    await initializeDatabase();

    const draftId = draft.draftId || generateUUID();
    const now = new Date().toISOString();

    const draftRecord = {
        draftId: draftId,
        longitude: draft.longitude,
        latitude: draft.latitude,
        observationType: draft.observationType,
        observationTime: draft.observationTime || now,
        notes: draft.notes || '',
        depthMeters: draft.depthMeters || null,
        speciesId: draft.speciesId || null,
        photoIds: draft.photoIds || [],
        status: 'draft',
        createdAt: draft.createdAt || now,
        updatedAt: now
    };

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.DRAFTS], 'readwrite');
        const store = transaction.objectStore(STORES.DRAFTS);
        const request = store.put(draftRecord);

        request.onsuccess = () => {
            console.log('[OfflineStorage] Draft saved:', draftId);
            resolve(draftId);
        };
        request.onerror = () => reject(request.error);
    });
}

/**
 * Get a draft by ID
 * @param {string} draftId - The draft ID
 * @returns {Promise<Object|null>} The draft or null
 */
async function getDraft(draftId) {
    await initializeDatabase();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.DRAFTS], 'readonly');
        const store = transaction.objectStore(STORES.DRAFTS);
        const request = store.get(draftId);

        request.onsuccess = () => resolve(request.result || null);
        request.onerror = () => reject(request.error);
    });
}

/**
 * Get all drafts
 * @returns {Promise<Array>} Array of drafts
 */
async function getAllDrafts() {
    await initializeDatabase();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.DRAFTS], 'readonly');
        const store = transaction.objectStore(STORES.DRAFTS);
        const request = store.getAll();

        request.onsuccess = () => resolve(request.result || []);
        request.onerror = () => reject(request.error);
    });
}

/**
 * Delete a draft and its associated photos
 * @param {string} draftId - The draft ID
 */
async function deleteDraft(draftId) {
    await initializeDatabase();

    return new Promise(async (resolve, reject) => {
        try {
            // First, delete associated photos
            await deletePhotosByDraftId(draftId);

            // Then delete the draft
            const transaction = db.transaction([STORES.DRAFTS], 'readwrite');
            const store = transaction.objectStore(STORES.DRAFTS);
            const request = store.delete(draftId);

            request.onsuccess = () => {
                console.log('[OfflineStorage] Draft deleted:', draftId);
                resolve();
            };
            request.onerror = () => reject(request.error);
        } catch (error) {
            reject(error);
        }
    });
}

/**
 * Save a photo for a draft
 * @param {string} draftId - The associated draft ID
 * @param {Blob} photoBlob - The photo blob
 * @param {string} fileName - Original filename
 * @param {Object} metadata - Photo metadata (EXIF GPS, etc.)
 * @returns {Promise<string>} The photo ID
 */
async function savePhoto(draftId, photoBlob, fileName, metadata = {}) {
    await initializeDatabase();

    const photoId = generateUUID();

    // Convert blob to ArrayBuffer for storage
    const arrayBuffer = await photoBlob.arrayBuffer();

    const photoRecord = {
        photoId: photoId,
        draftId: draftId,
        fileName: fileName,
        contentType: photoBlob.type,
        size: photoBlob.size,
        data: arrayBuffer,
        metadata: metadata,
        createdAt: new Date().toISOString()
    };

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.PHOTOS], 'readwrite');
        const store = transaction.objectStore(STORES.PHOTOS);
        const request = store.put(photoRecord);

        request.onsuccess = () => {
            console.log('[OfflineStorage] Photo saved:', photoId, 'for draft:', draftId);
            resolve(photoId);
        };
        request.onerror = () => reject(request.error);
    });
}

/**
 * Get photos for a draft
 * @param {string} draftId - The draft ID
 * @returns {Promise<Array>} Array of photo records
 */
async function getPhotosForDraft(draftId) {
    await initializeDatabase();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.PHOTOS], 'readonly');
        const store = transaction.objectStore(STORES.PHOTOS);
        const index = store.index('draftId');
        const request = index.getAll(draftId);

        request.onsuccess = () => {
            // Convert ArrayBuffer back to Blob for each photo
            const photos = (request.result || []).map(photo => ({
                ...photo,
                blob: new Blob([photo.data], { type: photo.contentType })
            }));
            resolve(photos);
        };
        request.onerror = () => reject(request.error);
    });
}

/**
 * Delete all photos for a draft
 * @param {string} draftId - The draft ID
 */
async function deletePhotosByDraftId(draftId) {
    await initializeDatabase();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.PHOTOS], 'readwrite');
        const store = transaction.objectStore(STORES.PHOTOS);
        const index = store.index('draftId');
        const request = index.getAllKeys(draftId);

        request.onsuccess = () => {
            const keys = request.result || [];
            keys.forEach(key => store.delete(key));
            console.log('[OfflineStorage] Deleted', keys.length, 'photos for draft:', draftId);
            resolve();
        };
        request.onerror = () => reject(request.error);
    });
}

/**
 * Queue a draft for synchronization
 * @param {string} draftId - The draft ID to sync
 */
async function queueForSync(draftId) {
    await initializeDatabase();

    const draft = await getDraft(draftId);
    if (!draft) {
        throw new Error('Draft not found: ' + draftId);
    }

    // Update draft status
    draft.status = 'pending_sync';
    await saveDraft(draft);

    // Add to sync queue
    const queueItem = {
        draftId: draftId,
        status: 'pending',
        queuedAt: new Date().toISOString(),
        attempts: 0,
        lastError: null
    };

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.SYNC_QUEUE], 'readwrite');
        const store = transaction.objectStore(STORES.SYNC_QUEUE);
        const request = store.put(queueItem);

        request.onsuccess = () => {
            console.log('[OfflineStorage] Draft queued for sync:', draftId);
            resolve(request.result);
        };
        request.onerror = () => reject(request.error);
    });
}

/**
 * Get pending sync items
 * @returns {Promise<Array>} Array of pending sync items
 */
async function getPendingSyncItems() {
    await initializeDatabase();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.SYNC_QUEUE], 'readonly');
        const store = transaction.objectStore(STORES.SYNC_QUEUE);
        const index = store.index('status');
        const request = index.getAll('pending');

        request.onsuccess = () => resolve(request.result || []);
        request.onerror = () => reject(request.error);
    });
}

/**
 * Mark a sync item as completed
 * @param {number} queueId - The queue item ID
 */
async function markSyncCompleted(queueId) {
    await initializeDatabase();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.SYNC_QUEUE], 'readwrite');
        const store = transaction.objectStore(STORES.SYNC_QUEUE);
        const getRequest = store.get(queueId);

        getRequest.onsuccess = () => {
            const item = getRequest.result;
            if (item) {
                item.status = 'completed';
                item.completedAt = new Date().toISOString();
                const updateRequest = store.put(item);
                updateRequest.onsuccess = () => resolve();
                updateRequest.onerror = () => reject(updateRequest.error);
            } else {
                resolve();
            }
        };
        getRequest.onerror = () => reject(getRequest.error);
    });
}

/**
 * Mark a sync item as failed
 * @param {number} queueId - The queue item ID
 * @param {string} error - Error message
 */
async function markSyncFailed(queueId, error) {
    await initializeDatabase();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.SYNC_QUEUE], 'readwrite');
        const store = transaction.objectStore(STORES.SYNC_QUEUE);
        const getRequest = store.get(queueId);

        getRequest.onsuccess = () => {
            const item = getRequest.result;
            if (item) {
                item.attempts = (item.attempts || 0) + 1;
                item.lastError = error;
                item.lastAttemptAt = new Date().toISOString();

                // Mark as failed after 3 attempts
                if (item.attempts >= 3) {
                    item.status = 'failed';
                }

                const updateRequest = store.put(item);
                updateRequest.onsuccess = () => resolve();
                updateRequest.onerror = () => reject(updateRequest.error);
            } else {
                resolve();
            }
        };
        getRequest.onerror = () => reject(getRequest.error);
    });
}

/**
 * Get storage statistics
 * @returns {Promise<Object>} Storage stats
 */
async function getStorageStats() {
    await initializeDatabase();

    const drafts = await getAllDrafts();
    const pendingSync = await getPendingSyncItems();

    let totalPhotoSize = 0;
    for (const draft of drafts) {
        const photos = await getPhotosForDraft(draft.draftId);
        for (const photo of photos) {
            totalPhotoSize += photo.size || 0;
        }
    }

    return {
        draftCount: drafts.length,
        pendingSyncCount: pendingSync.length,
        totalPhotoSize: totalPhotoSize,
        totalPhotoSizeMB: (totalPhotoSize / (1024 * 1024)).toFixed(2)
    };
}

/**
 * Clear all offline data
 */
async function clearAllData() {
    await initializeDatabase();

    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORES.DRAFTS, STORES.PHOTOS, STORES.SYNC_QUEUE], 'readwrite');

        transaction.objectStore(STORES.DRAFTS).clear();
        transaction.objectStore(STORES.PHOTOS).clear();
        transaction.objectStore(STORES.SYNC_QUEUE).clear();

        transaction.oncomplete = () => {
            console.log('[OfflineStorage] All offline data cleared');
            resolve();
        };
        transaction.onerror = () => reject(transaction.error);
    });
}

/**
 * Generate a UUID v4
 */
function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

// Export functions for Blazor interop
window.offlineStorage = {
    initializeDatabase,
    saveDraft,
    getDraft,
    getAllDrafts,
    deleteDraft,
    savePhoto,
    getPhotosForDraft,
    queueForSync,
    getPendingSyncItems,
    markSyncCompleted,
    markSyncFailed,
    getStorageStats,
    clearAllData
};

// Initialize database on load
initializeDatabase().catch(console.error);

// Listen for online/offline events
window.addEventListener('online', () => {
    console.log('[OfflineStorage] Connection restored - checking sync queue');
    // Trigger sync check via custom event
    window.dispatchEvent(new CustomEvent('coralledger:connectivity', { detail: { online: true } }));
});

window.addEventListener('offline', () => {
    console.log('[OfflineStorage] Connection lost - offline mode active');
    window.dispatchEvent(new CustomEvent('coralledger:connectivity', { detail: { online: false } }));
});
