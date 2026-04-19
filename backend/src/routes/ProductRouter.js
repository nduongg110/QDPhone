const express = require('express');
const router = express.Router();

const ProductController = require('../controllers/ProductController');
const { authMiddleWare } = require('../middleware/authMiddleWare');

// =======================
// CREATE PRODUCT
// =======================
router.post('/create', authMiddleWare, ProductController.createProduct);

// =======================
// UPDATE PRODUCT
// =======================
router.put('/update/:id', authMiddleWare, ProductController.updateProduct);

// =======================
// GET PRODUCT
// =======================
router.get('/get-details/:id', ProductController.getDetailsProduct);
router.get('/get-all', ProductController.getAllProduct);

// =======================
// DELETE PRODUCT
// =======================
router.delete('/delete/:id', authMiddleWare, ProductController.deleteProduct);

module.exports = router;