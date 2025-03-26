import { createClient } from 'redis';

export const redisClient = createClient({
  password: process.env.REDIS_PASSWORD,

  socket: {
    host: process.env.REDIS_HOST,
    port: parseInt(process.env.REDIS_PORT as string),
  },
});

redisClient.on('error', (error) => {
  console.error('REDIS::', error);
});

redisClient.on('connect', () => {
  console.log('Connected to Redis');
});

const connect = async () => {
  await redisClient.connect();
};

connect();
