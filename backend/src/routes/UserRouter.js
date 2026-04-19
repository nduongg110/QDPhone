const express = require("express");
const router = express.Router();

const userController = require('../controllers/UserController');
const { authMiddleWare, authUserMiddleWare } = require('../middleware/authMiddleWare');

// =======================
// AUTH
// =======================
router.post('/sign-up', userController.createUser);
router.post('/sign-in', userController.loginUser);
router.post('/refresh-token', userController.refreshToken); // 🔥 thêm

// =======================
// USER
// =======================
router.put('/update-user/:id', userController.updateUser);
router.delete('/delete-user/:id', authMiddleWare, userController.deleteUser);

// =======================
// GET USER
// =======================

// 🔥 chỉ admin mới xem được tất cả
router.get('/get-all', authMiddleWare, userController.getAllUser);

// 🔥 user hoặc admin đều xem được (nếu đúng id)
router.get('/get-details/:id', authUserMiddleWare, userController.getDetailsUser);

module.exports = router;