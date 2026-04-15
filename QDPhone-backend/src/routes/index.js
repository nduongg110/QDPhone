const userRouter = require('./UserRouter');

const routes = (app) => {
    app.use('/api', userRouter);
};

module.exports = routes;