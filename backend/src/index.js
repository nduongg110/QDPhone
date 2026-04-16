require('dotenv').config();
const express = require('express');
const sql = require('mssql');
const cors = require('cors');
const app = express();
const routes = require('./routes')
const bodyParser = require('body-parser')
app.use(cors());
app.use(express.json());


const config = {
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  server: process.env.DB_SERVER,
  database: process.env.DB_NAME,
  options: {
    encrypt: false,
    trustServerCertificate: true,
    enableArithAbort: true
  }
};

sql.connect(config)
  .then(() => {
    console.log("Connected to SQL Server!");
  })
  .catch(err => {
    console.error("Database connection failed:", err);
  });

app.use(express.json())
routes(app);

const PORT = process.env.PORT || 3001;
app.listen(PORT, () => {
  console.log(`Server running at http://localhost:${PORT}`);
});