const sql = require('mssql');

const createProduct = async (pool, product) => {
    const {
        name,
        image,
        type,
        price,
        countInStock,
        rating,
        description
    } = product;

    const result = await pool.request()
        .input('Name', sql.NVarChar(255), name)
        .input('Image', sql.NVarChar(sql.MAX), image)
        .input('Type', sql.NVarChar(100), type)
        .input('Price', sql.Decimal(18, 2), price)
        .input('CountInStock', sql.Int, countInStock)
        .input('Rating', sql.Decimal(3, 2), rating)
        .input('Description', sql.NVarChar(sql.MAX), description)
        .query(`
            INSERT INTO Products (
                Name,
                Image,
                Type,
                Price,
                CountInStock,
                Rating,
                Description
            )
            OUTPUT INSERTED.*
            VALUES (
                @Name,
                @Image,
                @Type,
                @Price,
                @CountInStock,
                @Rating,
                @Description
            )
        `);

    return result.recordset[0];
};

module.exports = {
    createProduct
};