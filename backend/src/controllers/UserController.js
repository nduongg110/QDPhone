const userService = require('../services/UserService');

const createUser = async (req, res) => {
    try {
        const response = await userService.createUser(req.body);
        return res.status(200).json(response);
    } catch (e) {
        return res.status(500).json({
            status: 'ERR',
            message: e.message
        });
    }
};

module.exports = {
    createUser
};