import { Router } from "express";
import Users from "./users";
import userRequireMiddleware from "../middleware/userRequire";

const router = Router();

router.use(userRequireMiddleware);
router.use("/users", Users);

export default router;
