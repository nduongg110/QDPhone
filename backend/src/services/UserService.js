const sql = require('mssql');
const bcrypt = require('bcrypt');
const { genneralAccessToken, genneralRefreshToken } = require('../services/JwtService');

// =======================
// CREATE USER (SIGN UP)
// =======================
const createUser = async (newUser) => {
    try {
        const { name, email, password, confirmPassword, phone } = newUser;

        // 1. Check password
        if (password !== confirmPassword) {
            return {
                status: 'ERR',
                message: 'Password không khớp'
            };
        }

        const pool = await sql.connect();

        // 2. Check email tồn tại
        const checkUser = await pool.request()
            .input('Email', sql.NVarChar, email)
            .query('SELECT * FROM Users WHERE Email = @Email');

        if (checkUser.recordset.length > 0) {
            return {
                status: 'ERR',
                message: 'Email already exists'
            };
        }

        // 3. Hash password
        const hashedPassword = await bcrypt.hash(password, 10);

        // 4. Insert user
        const result = await pool.request()
            .input('Name', sql.NVarChar, name)
            .input('Email', sql.NVarChar, email)
            .input('Password', sql.NVarChar, hashedPassword)
            .input('Phone', sql.NVarChar, phone)
            .query(`
                INSERT INTO Users (Name, Email, Password, Phone, AccessToken, RefreshToken)
                OUTPUT INSERTED.*
                VALUES (@Name, @Email, @Password, @Phone, '', '')
            `);

        return {
            status: 'OK',
            message: 'SUCCESS',
            data: result.recordset[0]
        };

    } catch (e) {
        throw e;
    }
};

// =======================
// LOGIN USER (SIGN IN)
// =======================
const loginUser = async (userLogin) => {
    try {
        const { email, password } = userLogin;

        const pool = await sql.connect();

        // 1. Check user tồn tại
        const result = await pool.request()
            .input('Email', sql.NVarChar, email)
            .query('SELECT * FROM Users WHERE Email = @Email');

        const user = result.recordset[0];

        if (!user) {
            return {
                status: 'ERR',
                message: 'User not found'
            };
        }

        // 2. So sánh password
        const isMatch = await bcrypt.compare(password, user.Password);

        if (!isMatch) {
            return {
                status: 'ERR',
                message: 'Password is incorrect'
            };
        }

        // 3. Tạo token
        const access_token = await genneralAccessToken({
            id: user.Id,
            email: user.Email
        });

        const refresh_token = await genneralRefreshToken({
            id: user.Id,
            email: user.Email
        });

        // 4. Lưu refresh token vào DB
        await pool.request()
            .input('RefreshToken', sql.NVarChar, refresh_token)
            .input('Id', sql.Int, user.Id)
            .query(`
                UPDATE Users
                SET RefreshToken = @RefreshToken
                WHERE Id = @Id
            `);

        return {
            status: 'OK',
            message: 'SUCCESS',
            access_token,
            refresh_token
        };

    } catch (e) {
        throw e;
    }
};

module.exports = {
    createUser,
    loginUser
};