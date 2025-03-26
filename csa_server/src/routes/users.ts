import { NextFunction, Request, Response, Router } from 'express';
import { redisClient } from '../redis-source';
import { generate_jwt, get_user, verify_jwt } from '../utils/user';
import { parse, serialize } from 'cookie';

const router = Router();

//import bcrypt from 'bcrypt';
router.post('/create_user', async (req: Request, res: Response, next: NextFunction) => {
    try { 
        const { mail, username, password } = req.body;

        if (!mail || !username || !password) {
            res.status(400).json({ message: 'mail or username or password missing' });
            return;
        }

        const data: Record<string, string> = {
            [`mail`]: mail,
            [`username`]: username,
            //[`password`]: await bcrypt.hash(password, 10),
            [`password`]: password,
        };

        const usernamePrefix = `comments:${username}`;

        await redisClient.hSet(usernamePrefix, data);

        res.status(201).json({ message: 'User added :)' });
    } catch (err) {
        next(err);
    }
});
router.post('/login', async (req: Request, res: Response, next: NextFunction) => {
    try {
        const { username, password } = req.body;

        if (!username || !password) {
            res.status(400).json({ message: 'Username and password are required' });
            return;
        }

        const userExists = await redisClient.hGet('users', username);
        if (!userExists) {
            res.status(400).json({ message: 'Invalid credentials' });
            return;
        }

        //const isPasswordValid = await bcrypt.compare(password, userExists); //not using bcrypt yet
        const isPasswordValid = userExists === password;
        if (!isPasswordValid) {
            res.status(400).json({ message: 'Invalid credentials' });
            return;
        }

        const token = await generate_jwt(username);

        const cookie = serialize('auth_token', token, {
            httpOnly: process.env.PRODUCTION == "true", // Cookie is only accessible on the server
            secure: true, // Set to true if served over HTTPS
            maxAge: 60 * 60 * 24 * 31, // 1 month expiration
            sameSite: process.env.PRODUCTION === "true" ? 'none' : 'lax',
            domain: process.env.PRODUCTION === "true" ? '.vest.li' : 'localhost',
            path: '/', // Cookie is accessible on all routes
        });

        res.setHeader('Set-Cookie', cookie);

        res.status(200).json({ message: 'Logged in' });
    } catch (err) {
        next(err);
    }
});


export default router;
