const sql = require('mssql');

const createUser = async (pool, user) => {
    const { name, email, password, phone } = user;

    const result = await pool.request()
        .input('Name', sql.NVarChar, name)
        .input('Email', sql.NVarChar, email)
        .input('Password', sql.NVarChar, password)
        .input('Phone', sql.NVarChar, phone)
        .query(`
            INSERT INTO Users (Name, Email, Password, Phone, AccessToken, RefreshToken)
            VALUES (@Name, @Email, @Password, @Phone, '', '')
        `);

    return result;
};

module.exports = {
    createUser
};