// Initialize MessageDb database
db = db.getSiblingDB('MessageDb');

// Create collections with indexes for better performance
db.createCollection('ProducedMessages');
db.createCollection('ConsumedMessages');

// Create indexes for better query performance
db.ProducedMessages.createIndex({ "createdAt": 1 });
db.ProducedMessages.createIndex({ "messageId": 1 });

db.ConsumedMessages.createIndex({ "consumedAt": 1 });
db.ConsumedMessages.createIndex({ "topic": 1, "partition": 1, "offset": 1 });

print('MongoDB initialization completed');