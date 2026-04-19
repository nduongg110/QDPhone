const sql = require('mssql');
const bcrypt = require('bcrypt');
const { genneralAccessToken, genneralRefreshToken } = require('../services/JwtService');

// =======================
// CREATE USER (SIGN UP)
// =======================
const createUser = async (newUser) => {
    try {
        const { name, email, password, confirmPassword, phone } = newUser;

        if (password !== confirmPassword) {
            return {
                status: 'ERR',
                message: 'Password không khớp'
            };
        }

        const pool = await sql.connect();

        const checkUser = await pool.request()
            .input('Email', sql.NVarChar, email)
            .query('SELECT * FROM Users WHERE Email = @Email');

        if (checkUser.recordset.length > 0) {
            return {
                status: 'ERR',
                message: 'Email already exists'
            };
        }

        const hashedPassword = await bcrypt.hash(password, 10);

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

        const user = result.recordset[0];
        delete user.Password;

        return {
            status: 'OK',
            message: 'SUCCESS',
            data: user
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

        const isMatch = await bcrypt.compare(password, user.Password);

        if (!isMatch) {
            return {
                status: 'ERR',
                message: 'Password is incorrect'
            };
        }

        // 🔥 TẠO TOKEN (FULL FIELD)
        const access_token = await genneralAccessToken({
            id: user.Id,
            email: user.Email,
            isAdmin: user.IsAdmin
        });

        const refresh_token = await genneralRefreshToken({
            id: user.Id,
            email: user.Email,
            isAdmin: user.IsAdmin
        });

        // 🔥 LƯU REFRESH TOKEN
        await pool.request()
            .input('RefreshToken', sql.NVarChar, refresh_token)
            .input('Id', sql.Int, user.Id)
            .query(`
                UPDATE Users
                SET RefreshToken = @RefreshToken
                WHERE Id = @Id
            `);

        delete user.Password;

        return {
            status: 'OK',
            message: 'SUCCESS',
            access_token,
            refresh_token,
            user
        };

    } catch (e) {
        throw e;
    }
};

// =======================
// UPDATE USER
// =======================
const updateUser = async (id, data) => {
    try {
        const pool = await sql.connect();

        const checkUser = await pool.request()
            .input('Id', sql.Int, id)
            .query('SELECT * FROM Users WHERE Id = @Id');

        const user = checkUser.recordset[0];

        if (!user) {
            return {
                status: 'ERR',
                message: 'The user is not defined'
            };
        }

        let { name, email, password, phone } = data;

        // Check email trùng
        if (email && email !== user.Email) {
            const checkEmail = await pool.request()
                .input('Email', sql.NVarChar, email)
                .query('SELECT * FROM Users WHERE Email = @Email');

            if (checkEmail.recordset.length > 0) {
                return {
                    status: 'ERR',
                    message: 'Email already exists'
                };
            }
        }

        // Hash password nếu có đổi
        let newPassword = user.Password;
        if (password && password.trim() !== '') {
            newPassword = await bcrypt.hash(password, 10);
        }

        const result = await pool.request()
            .input('Id', sql.Int, id)
            .input('Name', sql.NVarChar, name || user.Name)
            .input('Email', sql.NVarChar, email || user.Email)
            .input('Password', sql.NVarChar, newPassword)
            .input('Phone', sql.NVarChar, phone || user.Phone)
            .query(`
                UPDATE Users
                SET 
                    Name = @Name,
                    Email = @Email,
                    Password = @Password,
                    Phone = @Phone
                WHERE Id = @Id;

                SELECT * FROM Users WHERE Id = @Id;
            `);

        const updatedUser = result.recordset[0];
        delete updatedUser.Password;

        return {
            status: 'OK',
            message: 'SUCCESS',
            data: updatedUser
        };

    } catch (e) {
        throw e;
    }
};

// =======================
// DELETE USER
// =======================
const deleteUser = async (id) => {
    try {
        const pool = await sql.connect();

        const checkUser = await pool.request()
            .input('Id', sql.Int, id)
            .query('SELECT * FROM Users WHERE Id = @Id');

        if (checkUser.recordset.length === 0) {
            return {
                status: 'ERR',
                message: 'The user is not defined'
            };
        }

        await pool.request()
            .input('Id', sql.Int, id)
            .query('DELETE FROM Users WHERE Id = @Id');

        return {
            status: 'OK',
            message: 'Delete user success'
        };

    } catch (e) {
        throw e;
    }
};

// =======================
// GET ALL USER
// =======================
const getAllUser = async () => {
    try {
        const pool = await sql.connect();

        const result = await pool.request()
            .query('SELECT * FROM Users');

        const users = result.recordset.map(user => {
            delete user.Password;
            return user;
        });

        return {
            status: 'OK',
            message: 'SUCCESS',
            data: users
        };

    } catch (e) {
        throw e;
    }
};

// =======================
// GET DETAILS USER
// =======================
const getDetailsUser = async (id) => {
    try {
        const pool = await sql.connect();

        const result = await pool.request()
            .input('Id', sql.Int, id)
            .query('SELECT * FROM Users WHERE Id = @Id');

        const user = result.recordset[0];

        if (!user) {
            return {
                status: 'ERR',
                message: 'The user is not defined'
            };
        }

        delete user.Password;

        return {
            status: 'OK',
            message: 'SUCCESS',
            data: user
        };

    } catch (e) {
        throw e;
    }
};

module.exports = {
    createUser,
    loginUser,
    updateUser,
    deleteUser,
    getAllUser,
    getDetailsUser
};