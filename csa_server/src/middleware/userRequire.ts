import { redisClient } from "../redis-source";
import { verify_jwt } from "../utils/user";
import { parse } from 'cookie';

const userRequireMiddleware = async (req, res, next) => {
    const excludedRoutes = [
        'users/login',
        'users/create_user',
    ];

    const clientIp = req.headers['x-forwarded-for'] || req.ip;

    console.log('User Connected', clientIp);
    console.log('user connected', req.path);

    if (!excludedRoutes.some((route) => req.path.includes(route))) {
        try {
            const cookies = parse(req.headers.cookie || '');

            const token = cookies.auth_token;

            if (!token) {
                return res
                    .status(401)
                    .json({ error: 'Authorization token not provided.' });
            }

            const decoded = await verify_jwt(token);

            if (!decoded) {
                return res.status(401).json({ error: 'Invalid token.' });
            }

            const userExists = await redisClient.hGet('users', decoded.username);

            if (!userExists) {
                return res.status(404).json({ error: 'User not found.' });
            }

            req.user = userExists;
            next(); 
        } catch (error) {
            console.error('Error in userRequireMiddleware:', error);
            return res.status(500).json({ error: 'Internal Server Error' });
        }
    } else {
        next();
    }
};

export default userRequireMiddleware;
