const userService = require('../services/UserService');

// =======================
// SIGN UP
// =======================
const createUser = async (req, res) => {
    try {
        const { name, email, password, confirmPassword, phone } = req.body;

        const reg = /^\w+([-.+]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/;
        const isCheckEmail = reg.test(email);

        // Validate
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

        // Validate
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

module.exports = {
    createUser,
    loginUser
};