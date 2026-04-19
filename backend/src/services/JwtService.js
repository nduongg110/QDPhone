const jwt = require('jsonwebtoken');
const dotenv = require('dotenv');

dotenv.config();

// =======================
// GENERATE ACCESS TOKEN
// =======================
const genneralAccessToken = async (payload) => {
    const access_token = jwt.sign(
        { ...payload }, // payload trực tiếp (id, email, isAdmin)
        process.env.ACCESS_TOKEN,
        { expiresIn: '1h' }
    );

    return access_token;
};

// =======================
// GENERATE REFRESH TOKEN
// =======================
const genneralRefreshToken = async (payload) => {
    const refresh_token = jwt.sign(
        { ...payload },
        process.env.REFRESH_TOKEN,
        { expiresIn: '365d' }
    );

    return refresh_token;
};

// =======================
// REFRESH ACCESS TOKEN
// =======================
const refreshTokenJwtService = (token) => {
    return new Promise((resolve, reject) => {
        try {
            jwt.verify(token, process.env.REFRESH_TOKEN, async (err, user) => {
                if (err) {
                    return resolve({
                        status: 'ERROR',
                        message: 'The authentication'
                    });
                }

                // ⚠️ FIX: KHÔNG có payload nữa
                const access_token = await genneralAccessToken({
                    id: user?.id,
                    email: user?.email,
                    isAdmin: user?.isAdmin
                });

                return resolve({
                    status: 'OK',
                    message: 'SUCCESS',
                    access_token
                });
            });
        } catch (e) {
            reject(e);
        }
    });
};

module.exports = {
    genneralAccessToken,
    genneralRefreshToken,
    refreshTokenJwtService
};