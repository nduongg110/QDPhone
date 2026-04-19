const userService = require('../services/UserService');
const JwtService = require('../services/JwtService'); 

// =======================
// SIGN UP
// =======================
const createUser = async (req, res) => {
    try {
        const { name, email, password, confirmPassword, phone } = req.body;

        const reg = /^\w+([-.+]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/;
        const isCheckEmail = reg.test(email);

        if (!name || !email || !password || !confirmPassword || !phone) {
            return res.status(200).json({
                status: 'ERR',
                message: 'The input is required'
            });
        } else if (!isCheckEmail) {
            return res.status(200).json({
                status: 'ERR',
                message: 'Email is invalid'
            });
        } else if (password !== confirmPassword) {
            return res.status(200).json({
                status: 'ERR',
                message: 'Password does not match confirmPassword'
            });
        }

        const response = await userService.createUser(req.body);
        return res.status(200).json(response);

    } catch (e) {
        return res.status(500).json({
            status: 'ERR',
            message: e.message
        });
    }
};

// =======================
// SIGN IN (LOGIN)
// =======================
const loginUser = async (req, res) => {
    try {
        const { email, password } = req.body;

        const reg = /^\w+([-.+]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/;
        const isCheckEmail = reg.test(email);

        if (!email || !password) {
            return res.status(200).json({
                status: 'ERR',
                message: 'The input is required'
            });
        } else if (!isCheckEmail) {
            return res.status(200).json({
                status: 'ERR',
                message: 'Email is invalid'
            });
        }

        const response = await userService.loginUser(req.body);
        return res.status(200).json(response);

    } catch (e) {
        return res.status(500).json({
            status: 'ERR',
            message: e.message
        });
    }
};

// =======================
// REFRESH TOKEN 🔥
// =======================
const refreshToken = async (req, res) => {
    try {
        const token = req.body.refresh_token;

        if (!token) {
            return res.status(200).json({
                status: 'ERR',
                message: 'The token is required'
            });
        }

        const response = await JwtService.refreshTokenJwtService(token);
        return res.status(200).json(response);

    } catch (e) {
        return res.status(404).json({
            message: e.message
        });
    }
};

// =======================
// UPDATE USER
// =======================
const updateUser = async (req, res) => {
    try {
        const userId = req.params.id;
        const data = req.body;

        if (!userId) {
            return res.status(200).json({
                status: 'ERR',
                message: 'The userId is required'
            });
        }

        const response = await userService.updateUser(userId, data);
        return res.status(200).json(response);

    } catch (e) {
        return res.status(500).json({
            status: 'ERR',
            message: e.message
        });
    }
};

// =======================
// DELETE USER
// =======================
const deleteUser = async (req, res) => {
    try {
        const userId = req.params.id;

        if (!userId) {
            return res.status(200).json({
                status: 'ERR',
                message: 'The userId is required'
            });
        }

        const response = await userService.deleteUser(userId);
        return res.status(200).json(response);

    } catch (e) {
        return res.status(500).json({
            status: 'ERR',
            message: e.message
        });
    }
};

// =======================
// GET ALL USER
// =======================
const getAllUser = async (req, res) => {
    try {
        const response = await userService.getAllUser();
        return res.status(200).json(response);

    } catch (e) {
        return res.status(500).json({
            status: 'ERR',
            message: e.message
        });
    }
};

// =======================
// GET DETAILS USER
// =======================
const getDetailsUser = async (req, res) => {
    try {
        const userId = req.params.id;

        if (!userId) {
            return res.status(200).json({
                status: 'ERR',
                message: 'The userId is required'
            });
        }

        const response = await userService.getDetailsUser(userId);
        return res.status(200).json(response);

    } catch (e) {
        return res.status(500).json({
            status: 'ERR',
            message: e.message
        });
    }
};

module.exports = {
    createUser,
    loginUser,
    refreshToken, // 🔥 EXPORT
    updateUser,
    deleteUser,
    getAllUser,
    getDetailsUser
};