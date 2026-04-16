const sql = require('mssql');

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

        // 3. Insert user
        const result = await pool.request()
            .input('Name', sql.NVarChar, name)
            .input('Email', sql.NVarChar, email)
            .input('Password', sql.NVarChar, password)
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

module.exports = {
    createUser
};