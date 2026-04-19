const ProductService = require('../services/ProductService');

const createProduct = async (req, res) => {
  try {
    const { name, image, type, countInStock, price, rating, description } = req.body;

    if (!name || !image || !type || !countInStock || !price || !rating || !description) {
      return res.status(200).json({
        status: 'ERR',
        message: 'The input is required'
      });
    }

    const response = await ProductService.createProduct(req.body);
    return res.status(200).json(response);

  } catch (e) {
    return res.status(500).json({
      status: 'ERR',
      message: e.message
    });
  }
};

const updateProduct = async (req, res) => {
  try {
    const productId = req.params.id;
    const data = req.body;

    if (!productId) {
      return res.status(200).json({
        status: 'ERR',
        message: 'The productId is required'
      });
    }

    const response = await ProductService.updateProduct(productId, data);
    return res.status(200).json(response);

  } catch (e) {
    return res.status(500).json({
      status: 'ERR',
      message: e.message
    });
  }
};

const getDetailsProduct = async (req, res) => {
  try {
    const productId = req.params.id;

    if (!productId) {
      return res.status(200).json({
        status: 'ERR',
        message: 'The productId is required'
      });
    }

    const response = await ProductService.getDetailsProduct(productId);
    return res.status(200).json(response);

  } catch (e) {
    return res.status(500).json({
      status: 'ERR',
      message: e.message
    });
  }
};

const deleteProduct = async (req, res) => {
  try {
    const productId = req.params.id;

    if (!productId) {
      return res.status(200).json({
        status: 'ERR',
        message: 'The productId is required'
      });
    }

    const response = await ProductService.deleteProduct(productId);
    return res.status(200).json(response);

  } catch (e) {
    return res.status(500).json({
      status: 'ERR',
      message: e.message
    });
  }
};

const getAllProduct = async (req, res) => {
  try {
    const { limit, page, sort, order, filter, value } = req.query;

    const response = await ProductService.getAllProduct(
      Number(limit) || 10,
      Number(page) || 1,
      sort && order ? [sort, order] : null,
      filter && value ? [filter, value] : null
    );

    return res.status(200).json(response);

  } catch (e) {
    return res.status(500).json({
      status: 'ERR',
      message: e.message
    });
  }
};

module.exports = {
  createProduct,
  updateProduct,
  getDetailsProduct,
  deleteProduct,
  getAllProduct
};