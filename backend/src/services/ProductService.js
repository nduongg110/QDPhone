const sql = require('mssql');
const ProductModel = require('../models/ProductModel');

// =======================
// VALID COLUMN (chống SQL Injection)
// =======================
const VALID_COLUMNS = ['Name', 'Type', 'Price', 'Rating', 'CreatedAt'];

// =======================
// CREATE PRODUCT
// =======================
const createProduct = async (newProduct) => {
  try {
    const { name } = newProduct;
    const pool = await sql.connect();

    const checkProduct = await pool.request()
      .input('Name', sql.NVarChar(255), name)
      .query('SELECT * FROM Products WHERE Name = @Name');

    if (checkProduct.recordset.length > 0) {
      return {
        status: 'OK',
        message: 'The name of product is already'
      };
    }

    const newProductCreated = await ProductModel.createProduct(pool, newProduct);

    return {
      status: 'OK',
      message: 'SUCCESS',
      data: newProductCreated
    };

  } catch (e) {
    throw e;
  }
};

// =======================
// UPDATE PRODUCT
// =======================
const updateProduct = async (id, data) => {
  try {
    const pool = await sql.connect();

    const checkProduct = await pool.request()
      .input('Id', sql.Int, id)
      .query('SELECT * FROM Products WHERE Id = @Id');

    if (checkProduct.recordset.length === 0) {
      return {
        status: 'OK',
        message: 'The product is not defined'
      };
    }

    const { name, image, type, price, countInStock, rating, description } = data;

    await pool.request()
      .input('Id', sql.Int, id)
      .input('Name', sql.NVarChar(255), name)
      .input('Image', sql.NVarChar(sql.MAX), image)
      .input('Type', sql.NVarChar(100), type)
      .input('Price', sql.Decimal(18, 2), price)
      .input('CountInStock', sql.Int, countInStock)
      .input('Rating', sql.Decimal(3, 2), rating)
      .input('Description', sql.NVarChar(sql.MAX), description)
      .query(`
        UPDATE Products
        SET 
          Name = @Name,
          Image = @Image,
          Type = @Type,
          Price = @Price,
          CountInStock = @CountInStock,
          Rating = @Rating,
          Description = @Description,
          UpdatedAt = GETDATE()
        WHERE Id = @Id
      `);

    const updatedProduct = await pool.request()
      .input('Id', sql.Int, id)
      .query('SELECT * FROM Products WHERE Id = @Id');

    return {
      status: 'OK',
      message: 'SUCCESS',
      data: updatedProduct.recordset[0]
    };

  } catch (e) {
    throw e;
  }
};

// =======================
// GET DETAILS PRODUCT
// =======================
const getDetailsProduct = async (id) => {
  try {
    const pool = await sql.connect();

    const result = await pool.request()
      .input('Id', sql.Int, id)
      .query('SELECT * FROM Products WHERE Id = @Id');

    if (result.recordset.length === 0) {
      return {
        status: 'OK',
        message: 'The product is not defined'
      };
    }

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
// DELETE PRODUCT
// =======================
const deleteProduct = async (id) => {
  try {
    const pool = await sql.connect();

    const checkProduct = await pool.request()
      .input('Id', sql.Int, id)
      .query('SELECT * FROM Products WHERE Id = @Id');

    if (checkProduct.recordset.length === 0) {
      return {
        status: 'OK',
        message: 'The product is not defined'
      };
    }

    await pool.request()
      .input('Id', sql.Int, id)
      .query('DELETE FROM Products WHERE Id = @Id');

    return {
      status: 'OK',
      message: 'Delete product success'
    };

  } catch (e) {
    throw e;
  }
};

// =======================
// GET ALL PRODUCT (PAGINATION + FILTER + SORT)
// =======================
const getAllProduct = async (limit = 8, page = 1, sort, filter) => {
  try {
    const pool = await sql.connect();
    const offset = (page - 1) * limit;

    let whereClause = '';
    let orderByClause = 'ORDER BY CreatedAt DESC, UpdatedAt DESC';

    // =======================
    // FILTER
    // =======================
    const filterRequest = pool.request();

    if (filter && filter.length === 2) {
      const column = filter[0];
      const value = filter[1];

      if (!VALID_COLUMNS.includes(column)) {
        throw new Error('Invalid filter column');
      }

      whereClause = `WHERE ${column} LIKE @FilterValue`;
      filterRequest.input('FilterValue', sql.NVarChar, `%${value}%`);
    }

    // =======================
    // SORT
    // =======================
    if (sort && sort.length === 2) {
      const order = sort[0].toUpperCase() === 'ASC' ? 'ASC' : 'DESC';
      const column = sort[1];

      if (!VALID_COLUMNS.includes(column)) {
        throw new Error('Invalid sort column');
      }

      orderByClause = `ORDER BY ${column} ${order}, CreatedAt DESC`;
    }

    // =======================
    // TOTAL
    // =======================
    const totalQuery = `
      SELECT COUNT(*) as total FROM Products
      ${whereClause}
    `;

    const totalResult = await filterRequest.query(totalQuery);
    const totalProduct = totalResult.recordset[0].total;

    // =======================
    // DATA
    // =======================
    const dataRequest = pool.request();

    if (filter && filter.length === 2) {
      dataRequest.input('FilterValue', sql.NVarChar, `%${filter[1]}%`);
    }

    dataRequest.input('Offset', sql.Int, offset);
    dataRequest.input('Limit', sql.Int, limit);

    const dataQuery = `
      SELECT * FROM Products
      ${whereClause}
      ${orderByClause}
      OFFSET @Offset ROWS
      FETCH NEXT @Limit ROWS ONLY
    `;

    const result = await dataRequest.query(dataQuery);

    return {
      status: 'OK',
      message: 'Success',
      data: result.recordset,
      total: totalProduct,
      pageCurrent: page,
      totalPage: Math.ceil(totalProduct / limit)
    };

  } catch (e) {
    throw e;
  }
};

module.exports = {
  createProduct,
  updateProduct,
  getDetailsProduct,
  deleteProduct,
  getAllProduct
};