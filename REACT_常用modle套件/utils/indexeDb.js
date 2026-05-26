// import { openDB } from 'idb';

// const DB_NAME = 'OfflineSyncDB';
// const DB_VERSION = 2; // 升級版本
// const STORE_CATHODE = 'pendingCathode';
// const STORE_ANODE = 'pendingAnode';

// /**
//  * 打開或創建 IndexedDB 資料庫
//  */
// export async function getDb() {
//     return openDB(DB_NAME, DB_VERSION, {
//         upgrade(db) {
//             // Cathode
//             if (!db.objectStoreNames.contains(STORE_CATHODE)) {
//                 const cathodeStore = db.createObjectStore(STORE_CATHODE, { keyPath: 'id', autoIncrement: true });
//                 cathodeStore.createIndex('originalClientTimestamp', 'originalClientTimestamp');
//             } else {
//                 const cathodeStore = db.transaction.objectStore(STORE_CATHODE);
//                 if (!cathodeStore.indexNames.contains('originalClientTimestamp')) {
//                     cathodeStore.createIndex('originalClientTimestamp', 'originalClientTimestamp');
//                 }
//             }
//             // Anode
//             if (!db.objectStoreNames.contains(STORE_ANODE)) {
//                 const anodeStore = db.createObjectStore(STORE_ANODE, { keyPath: 'id', autoIncrement: true });
//                 anodeStore.createIndex('originalClientTimestamp', 'originalClientTimestamp');
//             } else {
//                 const anodeStore = db.transaction.objectStore(STORE_ANODE);
//                 if (!anodeStore.indexNames.contains('originalClientTimestamp')) {
//                     anodeStore.createIndex('originalClientTimestamp', 'originalClientTimestamp');
//                 }
//             }
//         },
//     });
// }

// export async function addPendingCathode(data) {
//     const db = await getDb();
//     return db.put(STORE_CATHODE, { ...data, status: 'pending', originalClientTimestamp: Date.now() });
// }
// export async function addPendingAnode(data) {
//     const db = await getDb();
//     return db.put(STORE_ANODE, { ...data, status: 'pending', originalClientTimestamp: Date.now() });
// }

// /**
//  * 獲取所有待同步的資料 (狀態為 'pending')
//  */
// export async function getPendingSubmissions() {
//     const db = await getDb();
//     const cathodeItems = await db.getAll(STORE_CATHODE);
//     const anodeItems = await db.getAll(STORE_ANODE);
//     return {
//         cathode: cathodeItems.filter(item => item.status === 'pending'),
//         anode: anodeItems.filter(item => item.status === 'pending')
//     };
// }

// /**
//  * 根據 ID 更新資料的狀態
//  */
// export async function updateSubmissionStatus(id, newStatus) {
//     const db = await getDb();
//     let item = await db.get(STORE_CATHODE, id);
//     if (item) {
//         item.status = newStatus;
//         await db.put(STORE_CATHODE, item);
//         return;
//     }
//     item = await db.get(STORE_ANODE, id);
//     if (item) {
//         item.status = newStatus;
//         await db.put(STORE_ANODE, item);
//         return;
//     }
// }

// /**
//  * 清理 IndexedDB 中已同步的舊資料 (針對兩個 store)
//  */
// export async function cleanupSyncedSubmissions() {
//     const db = await getDb();
//     const cleanupPoint = calculateFrontendTimeForCleanup().getTime();

//     let deletedCount = 0;
//     for (const storeName of [STORE_CATHODE, STORE_ANODE]) {
//         const tx = db.transaction(storeName, 'readwrite');
//         const store = tx.objectStore(storeName);
//         const index = store.index('originalClientTimestamp');
//         let cursor = await index.openCursor(IDBKeyRange.upperBound(cleanupPoint));
        
        
//         while (cursor) {
//             if (cursor.value.status === 'synced') {
//                 await cursor.delete();
//                 deletedCount++;
//             }
//             cursor = await cursor.continue();
//         }
//         await tx.done;
//     }
//     console.log(`[Frontend IDB] Cleaned up ${deletedCount} old synced records.`);
//     return deletedCount;
// }

// /**
//  * 輔助函數：計算前端清理的目標時間點 (本週三 12:45)
//  */
// function calculateFrontendTimeForCleanup() {
//     const now = new Date();
//     const options = {
//         timeZone: 'Asia/Taipei',
//         year: 'numeric', month: 'numeric', day: 'numeric',
//         hour: 'numeric', minute: 'numeric', second: 'numeric', hour12: false
//     };
//     const nowInTaipeiString = now.toLocaleString('en-US', options);
//     const nowInTaipei = new Date(nowInTaipeiString);

//     const targetDayOfWeek = 3; // 週三
//     let cleanupPoint = new Date(nowInTaipei);
//     cleanupPoint.setDate(nowInTaipei.getDate() + (targetDayOfWeek - nowInTaipei.getDay() + 7) % 7);
//     cleanupPoint.setHours(12, 45, 0, 0);

//     if (nowInTaipei.getTime() < cleanupPoint.getTime()) {
//         cleanupPoint.setDate(cleanupPoint.getDate() - 7);
//     }
//     return cleanupPoint;
// }
