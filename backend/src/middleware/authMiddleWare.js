const jwt = require('jsonwebtoken');

// =======================
// ADMIN ONLY
// =======================
const authMiddleWare = (req, res, next) => {
  try {
    const authHeader = req.headers.authorization;

    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({
        status: 'ERROR',
        message: 'Token is required'
      });
    }

    const token = authHeader.split(' ')[1];

    const decoded = jwt.verify(token, process.env.ACCESS_TOKEN);

    if (decoded?.isAdmin) {
      req.user = decoded; // 🔥 gắn user vào req để dùng sau
      next();
    } else {
      return res.status(403).json({
        status: 'ERROR',
        message: 'You are not authorized'
      });
    }
  } catch (err) {
    return res.status(403).json({
      status: 'ERROR',
      message: 'Invalid token'
    });
  }
};

// =======================
// USER OR ADMIN
// =======================
const authUserMiddleWare = (req, res, next) => {
  try {
    const authHeader = req.headers.authorization;
    const userId = req.params.id;

    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({
        status: 'ERROR',
        message: 'Token is required'
      });
    }

    const token = authHeader.split(' ')[1];

    const decoded = jwt.verify(token, process.env.ACCESS_TOKEN);

    if (decoded?.isAdmin || decoded?.id == userId) {
      req.user = decoded; // 🔥 gắn user
      next();
    } else {
      return res.status(403).json({
        status: 'ERROR',
        message: 'You are not authorized'
      });
    }
  } catch (err) {
    return res.status(403).json({
      status: 'ERROR',
      message: 'Invalid token'
    });
  }
};

module.exports = {
  authMiddleWare,
  authUserMiddleWare
};